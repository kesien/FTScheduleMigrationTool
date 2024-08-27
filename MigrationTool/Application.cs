using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MigrationTool.Constants;
using MigrationTool.Persistency;
using NodaTime;
using Npgsql;

namespace MigrationTool;

public record Migration(string MigrationId, string ProductVersion);

public class Application
{
    private readonly OldApplicationDbContext _context;
    private readonly NewApplicationDbContext _newContext;

    public Application(OldApplicationDbContext context, NewApplicationDbContext newContext)
    {
        _context = context;
        _newContext = newContext;
    }
    public async Task Migrate()
    {
        await _newContext.Database.EnsureDeletedAsync();
        await _newContext.Database.EnsureCreatedAsync();
        var migrations = _context.Database.SqlQueryRaw<Migration>("SELECT * FROM public.\"__EFMigrationsHistory\"").ToList();

        await _newContext.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE ""__EFMigrationsHistory"" (
            ""MigrationId"" varchar(150), 
            ""ProductVersion"" varchar(32)
        );");
        foreach (var migration in migrations)
        {
            await _newContext.Database.ExecuteSqlRawAsync(@"
        INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"") 
        VALUES (@MigrationId, @ProductVersion)",
        new[] {
            new NpgsqlParameter("MigrationId", migration.MigrationId),
            new NpgsqlParameter("ProductVersion", migration.ProductVersion)
        });
        }

        var departments = await _context.Departments.AsNoTracking().ToListAsync();
        var todos = await _context.ToDos.AsNoTracking().ToListAsync();
        var userRoles = await _context.UserRoles.AsNoTracking().ToListAsync();
        var roles = await _context.Roles.AsNoTracking().ToListAsync();
        var appLanguages = await _context.ApplicationLanguages.AsNoTracking().ToListAsync();
        var users = await _context.Users.Include(u => u.Absences).Include(u => u.Departments).Include(u => u.Requests).Include(u => u.Requests).Include(u => u.WorkAssignments).ThenInclude(wa => wa.Todos).Include(u => u.Absences).AsNoTracking().ToListAsync();
        var seats = await _context.Seats.AsNoTracking().ToListAsync();
        var locations = await _context.Locations.Include(l => l.Seats).AsNoTracking().ToListAsync();

        var newSeats = seats.Select(s => new NewSeatEntity { Id = s.Id, Name = s.Name, PhoneNumber = s.PhoneNumber, LocationId = s.LocationId }).ToList();
        var newDepartments = departments.Select(d => new NewDepartmentEntity { Id = d.Id, Name = d.Name }).ToList();
        var newTodos = todos.Select(t => new NewToDoEntity { Id = t.Id, Name = t.Name, Description = t.Description }).ToList();
        var newLocations = locations.Select(l => new NewLocationEntity { Address = l.Address, Id = l.Id, Name = l.Name, Seats = newSeats.Where(newSeat => l.Seats.Select(seat => seat.Id).Any(seatId => seatId == newSeat.Id)).ToList() }).ToList();

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"Migrating {newLocations.Count} locations");
        await _newContext.Locations.AddRangeAsync(newLocations);
        await _newContext.SaveChangesAsync();
        Console.WriteLine($"Migrating {roles.Count} roles");
        await _newContext.Roles.AddRangeAsync(roles);
        await _newContext.SaveChangesAsync();
        Console.WriteLine($"Migrating {departments.Count} departments");
        await _newContext.Departments.AddRangeAsync(newDepartments);
        await _newContext.SaveChangesAsync();
        Console.WriteLine($"Migrating {todos.Count} todos");
        await _newContext.ToDos.AddRangeAsync(newTodos);
        await _newContext.SaveChangesAsync();
        Console.WriteLine($"Migrating {appLanguages.Count} app languages");
        await _newContext.ApplicationLanguages.AddRangeAsync(appLanguages);
        await _newContext.SaveChangesAsync();

        var newUsers = new List<NewUserEntity>();
        foreach (var user in users)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Migrating {user.Email} with {user.WorkAssignments.Count} work assignments and {user.Absences.Count} absences");
            var newUserEntity = new NewUserEntity
            {
                AccessFailedCount = user.AccessFailedCount,
                ConcurrencyStamp = user.ConcurrencyStamp,
                Departments = newDepartments.Where(newD => user.Departments.Select(d => d.Id).Any(x => x == newD.Id)).ToList(),
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                FirstName = user.FirstName,
                Id = user.Id,
                LastName = user.LastName,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                NormalizedEmail = user.NormalizedEmail,
                NormalizedUserName = user.NormalizedUserName,
                PasswordHash = user.PasswordHash,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                RefreshToken = user.RefreshToken,
                RefreshTokenExpires = null,
                SecurityStamp = user.SecurityStamp,
                TwoFactorEnabled = user.TwoFactorEnabled,
                UserName = user.UserName,
            };

            newUsers.Add(newUserEntity);
        }

        await _newContext.Users.AddRangeAsync(newUsers);
        await _newContext.SaveChangesAsync();

        foreach (var user in users)
        {
            var newUser = newUsers.FirstOrDefault(u => u.Id == user.Id);

            foreach (var request in user.Requests)
            {
                var seatEntity = newSeats.FirstOrDefault(s => s.Id == request.SeatId);
                var locationEntity = newLocations.FirstOrDefault(l => l.Seats.Any(s => s.Id == request.SeatId));

                var eventEntity = new EventEntity
                {
                    Date = DateTime.SpecifyKind(request.StartTime.Date, DateTimeKind.Utc),
                    DtStart = DateTime.SpecifyKind(request.StartTime.AddHours(-2), DateTimeKind.Utc),//request.StartTime.ToUniversalTime(),
                    DtEnd = DateTime.SpecifyKind(request.EndTime.AddHours(-2), DateTimeKind.Utc),
                    Description = "",
                    EventType = EventType.Event,
                    IsAllDay = false,
                    Summary = seatEntity?.Name ?? "",
                    Todos = "",
                    SeatId = seatEntity?.Id ?? Guid.Empty,
                    Location = locationEntity?.Name ?? "",
                    LocationId = locationEntity?.Id ?? Guid.Empty,
                    PhoneNumber = seatEntity?.PhoneNumber,
                    Uid = Guid.NewGuid(),
                    RecurrenceId = null,
                    TimezoneId = "UTC"
                };
                if (request.IsFix)
                {
                    RecurrenceEntity recurrenceEntity = new()
                    {
                        Event = eventEntity,
                        Interval = 1,
                        Frequency = "weekly",
                        ByMonthDay = new(),
                        ByDay = [request.DayIndex == 6 ? 0 : request.DayIndex + 1],
                        ByHour = new(),
                        ByMinute = new(),
                        BySecond = new(),
                        ByWeekNo = new(),
                        ByMonth = new(),
                        ByYearDay = new(),
                        BySetPosition = new(),
                    };
                    recurrenceEntity.RecurrencePattern = recurrenceEntity.GetRecurrencePattern();
                    eventEntity.Recurrences.Add(recurrenceEntity);
                }
                newUser?.Calendar.Events.Add(eventEntity);
            }

            foreach (var wa in user.WorkAssignments)
            {
                if (wa.IsRequest) continue;
                var seatEntity = newSeats.FirstOrDefault(s => s.Id == wa.SeatId);
                var locationEntity = newLocations.FirstOrDefault(l => l.Seats.Any(s => s.Id == wa.SeatId));
                var eventEntity = new EventEntity
                {
                    Date = DateTime.SpecifyKind(wa.StartDate.Date, DateTimeKind.Utc),
                    DtStart = DateTime.SpecifyKind(wa.StartDate.AddHours(-2), DateTimeKind.Utc),
                    DtEnd = DateTime.SpecifyKind(wa.EndDate.AddHours(-2), DateTimeKind.Utc),
                    Description = wa.Comment,
                    EventType = EventType.Event,
                    IsAllDay = false,
                    Summary = seatEntity?.Name ?? "",
                    Todos = JsonSerializer.Serialize(newTodos.Where(newTodo => wa.Todos.Select(t => t.Id).Any(todoid => todoid == newTodo.Id)), new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }),
                    SeatId = seatEntity?.Id ?? Guid.Empty,
                    Location = locationEntity?.Name ?? "",
                    LocationId = locationEntity?.Id ?? Guid.Empty,
                    PhoneNumber = seatEntity?.PhoneNumber,
                    Uid = Guid.NewGuid(),
                    RecurrenceId = null,
                    TimezoneId = "UTC"
                };
                newUser?.Calendar.Events.Add(eventEntity);
            };

            foreach (var absence in user.Absences)
            {
                var curDate = absence.StartDate;
                while (curDate <= absence.EndDate)
                {

                    var eventForSameDay = newUser?.Calendar.Events.FirstOrDefault(e => e.Recurrences.Count > 0 && e.Date.Value.DayOfWeek == curDate.Date.DayOfWeek);
                    if (eventForSameDay is not null)
                    {
                        var exception = new RecurrenceExceptionEntity
                        {
                            Date = DateTime.SpecifyKind(curDate.Date, DateTimeKind.Utc),
                            Event = eventForSameDay
                        };
                        eventForSameDay.Exceptions.Add(exception);

                    }
                    curDate = curDate.AddDays(1);
                }
                var eventEntity = new EventEntity
                {
                    Date = DateTime.SpecifyKind(absence.StartDate.Date, DateTimeKind.Utc),
                    DtStart = DateTime.SpecifyKind(absence.StartDate, DateTimeKind.Utc),
                    DtEnd = absence.StartDate.AddDays(1) != absence.EndDate ? DateTime.SpecifyKind(absence.StartDate.AddDays(1), DateTimeKind.Utc) : DateTime.SpecifyKind(absence.EndDate, DateTimeKind.Utc),
                    Description = "",
                    EventType = GetEventType(absence.Type),
                    IsAllDay = true,
                    Summary = GetEventType(absence.Type).ToString(),
                    Todos = "",
                    SeatId = Guid.Empty,
                    Location = "",
                    LocationId = Guid.Empty,
                    PhoneNumber = "",
                    RecurrenceId = null,
                    Uid = Guid.NewGuid(),
                    TimezoneId = "UTC"
                };

                if (absence.StartDate.AddDays(1) != absence.EndDate)
                {
                    RecurrenceEntity recurrenceEntity = new()
                    {
                        Event = eventEntity,
                        Interval = 1,
                        Frequency = "daily",
                        ByMonthDay = new(),
                        ByDay = [],
                        ByHour = new(),
                        ByMinute = new(),
                        BySecond = new(),
                        ByWeekNo = new(),
                        ByMonth = new(),
                        ByYearDay = new(),
                        BySetPosition = new(),
                        Until = DateTime.SpecifyKind(absence.EndDate, DateTimeKind.Utc)
                    };
                    recurrenceEntity.RecurrencePattern = recurrenceEntity.GetRecurrencePattern();
                    eventEntity.Recurrences.Add(recurrenceEntity);
                }

                newUser?.Calendar.Events.Add(eventEntity);
            }
        }
        await _newContext.SaveChangesAsync();

        Console.WriteLine($"Migrating {userRoles.Count} user roles");
        await _newContext.AddRangeAsync(userRoles);
        await _newContext.SaveChangesAsync();
        await _newContext.Database.ExecuteSqlRawAsync(@"ALTER TABLE ""NewDepartmentEntityNewUserEntity"" RENAME TO ""DepartmentEntityUserEntity"";");
    }

    private EventType GetEventType(AbsenceType type) => type switch
    {
        AbsenceType.Holiday => EventType.Holiday,
        AbsenceType.HomeOffice => EventType.HomeOffice,
        AbsenceType.OutOfOffice => EventType.OutOfOffice,
        AbsenceType.School => EventType.School,
        _ => EventType.Holiday
    };
}
