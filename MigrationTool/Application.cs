using Microsoft.EntityFrameworkCore;
using MigrationTool.Constants;
using MigrationTool.Persistency;
using Npgsql;
using System.Text.Json;

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

        // Egyszerű entitások migrálása (változatlan)
        var departments = await _context.Departments.AsNoTracking().ToListAsync();
        var todos = await _context.ToDos.AsNoTracking().ToListAsync();
        var userRoles = await _context.UserRoles.AsNoTracking().ToListAsync();
        var roles = await _context.Roles.AsNoTracking().ToListAsync();
        var appLanguages = await _context.ApplicationLanguages.AsNoTracking().ToListAsync();
        var seats = await _context.Seats.AsNoTracking().ToListAsync();
        var locations = await _context.Locations.Include(l => l.Seats).AsNoTracking().ToListAsync();

        var newSeats = seats.Select(s => new NewSeatEntity { Id = s.Id, Name = s.Name, PhoneNumber = s.PhoneNumber, LocationId = s.LocationId }).ToList();
        var newDepartments = departments.Select(d => new NewDepartmentEntity { Id = d.Id, Name = d.Name }).ToList();
        var newTodos = todos.Select(t => new NewToDoEntity { Id = t.Id, Name = t.Name, Description = t.Description }).ToList();
        var newLocations = locations.Select(l => new NewLocationEntity { Address = l.Address, Id = l.Id, Name = l.Name, Seats = newSeats.Where(newSeat => l.Seats.Select(seat => seat.Id).Any(seatId => seatId == newSeat.Id)).ToList() }).ToList();

        // Migration history kezelése
        var migrations = _context.Database.SqlQueryRaw<Migration>("SELECT * FROM public.\"__EFMigrationsHistory\"").ToList();

        await _newContext.Database.ExecuteSqlRawAsync(@"
    CREATE TABLE ""__EFMigrationsHistory"" (
        ""MigrationId"" varchar(150), 
        ""ProductVersion"" varchar(32)
    );");
        migrations.Add(new Migration("20240807071618_CalendarAdded", "7.0.10"));
        migrations.Add(new Migration("20240808185311_EventTypeAdded", "7.0.10"));
        migrations.Add(new Migration("20240810185254_PhoneNumberAddedToEvent", "7.0.10"));
        migrations.Add(new Migration("20240813102839_DateAddedToEventEntity", "7.0.10"));
        migrations.Add(new Migration("20240814071640_UidAddedToEventEntity", "7.0.10"));
        migrations.Add(new Migration("20240814115358_RecurrenceIdAddedToEventEntity", "7.0.10"));

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

        // USERS MIGRÁLÁSA CHUNK-OKBAN
        const int batchSize = 100; // Állítsd be a rendszered kapacitása szerint
        var totalUsers = await _context.Users.CountAsync();
        Console.WriteLine($"Total users to migrate: {totalUsers}");

        for (int skip = 0; skip < totalUsers; skip += batchSize)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Processing users batch {skip / batchSize + 1} ({skip + 1}-{Math.Min(skip + batchSize, totalUsers)} of {totalUsers})");

            // Felhasználók betöltése chunk-okban
            var usersBatch = await _context.Users
                .Include(u => u.Departments)
                .Skip(skip)
                .Take(batchSize)
                .AsNoTracking()
                .ToListAsync();

            var newUsersBatch = new List<NewUserEntity>();

            foreach (var user in usersBatch)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"Migrating user: {user.Email}");

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

                newUsersBatch.Add(newUserEntity);
            }

            // Felhasználók mentése
            await _newContext.Users.AddRangeAsync(newUsersBatch);
            await _newContext.SaveChangesAsync();

            // Most dolgozzuk fel a kapcsolódó adatokat külön lekérdezésekkel
            await ProcessUserRelatedData(usersBatch.Select(u => u.Id).ToList(), newSeats, newLocations, newTodos);

            // Memória felszabadítása
            _newContext.ChangeTracker.Clear();
            GC.Collect();
        }

        // UserRoles migrálása
        Console.WriteLine($"Migrating {userRoles.Count} user roles");
        await _newContext.AddRangeAsync(userRoles);
        await _newContext.SaveChangesAsync();
        await _newContext.Database.ExecuteSqlRawAsync(@"ALTER TABLE ""NewDepartmentEntityNewUserEntity"" RENAME TO ""DepartmentEntityUserEntity"";");
    }

    private async Task ProcessUserRelatedData(List<Guid> userIds, List<NewSeatEntity> newSeats, List<NewLocationEntity> newLocations, List<NewToDoEntity> newTodos)
    {
        foreach (var userId in userIds)
        {
            Console.WriteLine($"Processing related data for user: {userId}");

            // Külön lekérdezések a kapcsolódó adatokhoz
            var userRequests = await _context.Requests
                .Where(r => r.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            var userWorkAssignments = await _context.WorkAssignments
                .Include(wa => wa.Todos)
                .Where(wa => wa.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            var userAbsences = await _context.Absences
                .Where(a => a.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            var newUser = await _newContext.Users.FirstAsync(u => u.Id == userId);

            // Requests feldolgozása
            foreach (var request in userRequests)
            {
                var seatEntity = newSeats.FirstOrDefault(s => s.Id == request.SeatId);
                var locationEntity = newLocations.FirstOrDefault(l => l.Seats.Any(s => s.Id == request.SeatId));

                var eventEntity = new EventEntity
                {
                    Date = DateTime.SpecifyKind(request.StartTime.Date, DateTimeKind.Utc),
                    DtStart = DateTime.SpecifyKind(request.StartTime.AddHours(-2), DateTimeKind.Utc),
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
                newUser.Calendar.Events.Add(eventEntity);
            }

            // WorkAssignments feldolgozása
            var processedEvents = new HashSet<string>(); // Key: "Date-SeatId-StartTime-EndTime"
            foreach (var wa in userWorkAssignments)
            {
                var seatEntity = newSeats.FirstOrDefault(s => s.Id == wa.SeatId);
                var locationEntity = newLocations.FirstOrDefault(l => l.Seats.Any(s => s.Id == wa.SeatId));

                // Deduplikációs kulcs
                var eventKey = $"{wa.StartDate.Date:yyyy-MM-dd}-{wa.SeatId}-{wa.StartDate.TimeOfDay}-{wa.EndDate.TimeOfDay}";

                if (processedEvents.Contains(eventKey))
                {
                    Console.WriteLine($"Skipping duplicate WorkAssignment: {eventKey}");
                    continue; // Skip duplicate
                }

                processedEvents.Add(eventKey);

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


                // HA ez egy IsRequest WorkAssignment, akkor keress recurring event-et ugyanarra a DayOfWeek-re és SeatId-re
                if (wa.IsRequest)
                {
                    // Keress recurring event-et ugyanarra a napra (DayOfWeek) és ugyanarra a seat-re
                    var recurringEventForSameDay = newUser.Calendar.Events.FirstOrDefault(e =>
                        e.Recurrences.Any() &&
                        e.Date.HasValue &&
                        e.Date.Value.DayOfWeek == wa.StartDate.Date.DayOfWeek &&
                        e.SeatId == wa.SeatId);

                    if (recurringEventForSameDay is not null)
                    {
                        Console.WriteLine($"Adding exception for WorkAssignment on {wa.StartDate.Date} (SeatId: {wa.SeatId}) to recurring event on {recurringEventForSameDay.Date.Value.DayOfWeek}");
                        recurringEventForSameDay.Exceptions.Add(new RecurrenceExceptionEntity
                        {
                            Date = DateTime.SpecifyKind(wa.StartDate.Date, DateTimeKind.Utc),
                            Event = recurringEventForSameDay
                        });
                    }
                }

                newUser.Calendar.Events.Add(eventEntity);
            }

            // Absences feldolgozása
            foreach (var absence in userAbsences)
            {
                var curDate = absence.StartDate;
                while (curDate <= absence.EndDate)
                {
                    var eventForSameDay = newUser.Calendar.Events.FirstOrDefault(e => e.Recurrences.Count > 0 && e.Date.Value.DayOfWeek == curDate.Date.DayOfWeek);
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

                newUser.Calendar.Events.Add(eventEntity);
            }

            await _newContext.SaveChangesAsync();
        }
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
