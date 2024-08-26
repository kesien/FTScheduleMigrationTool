using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MigrationTool.Constants;
using MigrationTool.Persistency;

namespace MigrationTool;

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
        var users = await _context.Users.Include(u => u.Absences).Include(u => u.Departments).Include(u => u.Requests).Include(u => u.WorkAssignments).ThenInclude(wa => wa.Todos).Include(u => u.Absences).ToListAsync();
        var seats = await _context.Seats.AsNoTracking().ToListAsync();
        var locations = await _context.Locations.Include(l => l.Seats).AsNoTracking().ToListAsync();
        foreach (var user in users)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Migrating {user.Email} with {user.WorkAssignments.Count} work assignments and {user.Absences.Count} absences");
            var newUserEntity = new NewUserEntity
            {
                AccessFailedCount = user.AccessFailedCount,
                ConcurrencyStamp = user.ConcurrencyStamp,
                Departments = user.Departments,
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
                RefreshTokenExpires = user.RefreshTokenExpires,
                SecurityStamp = user.SecurityStamp,
                TwoFactorEnabled = user.TwoFactorEnabled,
                UserName = user.UserName
            };

            foreach (var wa in user.WorkAssignments)
            {
                var seatEntity = seats.FirstOrDefault(s => s.Id == wa.SeatId);
                var locationEntity = locations.FirstOrDefault(l => l.Seats.Any(s => s.Id == wa.SeatId));
                var eventEntity = new EventEntity
                {
                    Date = wa.StartDate.Date,
                    DtStart = wa.StartDate,
                    DtEnd = wa.EndDate,
                    Description = wa.Comment,
                    EventType = EventType.Event,
                    IsAllDay = false,
                    Summary = seatEntity?.Name ?? "",
                    Todos = JsonSerializer.Serialize(wa.Todos),
                    SeatId = seatEntity?.Id ?? Guid.Empty,
                    Location = locationEntity?.Name ?? "",
                    LocationId = locationEntity?.Id ?? Guid.Empty,
                    PhoneNumber = seatEntity?.PhoneNumber,
                    Uid = Guid.NewGuid()
                };
                newUserEntity.Calendar.Events.Add(eventEntity);
            };

            foreach (var absence in user.Absences)
            {
                var eventEntity = new EventEntity
                {
                    Date = absence.StartDate.Date,
                    DtStart = absence.StartDate,
                    DtEnd = absence.EndDate,
                    Description = "",
                    EventType = GetEventType(absence.Type),
                    IsAllDay = true,
                    Summary = GetEventType(absence.Type).ToString(),
                    Todos = "",
                    SeatId = Guid.Empty,
                    Location = "",
                    LocationId = Guid.Empty,
                    PhoneNumber = "",
                    Uid = Guid.NewGuid()
                };

                newUserEntity.Calendar.Events.Add(eventEntity);
            }

            await _newContext.Users.AddAsync(newUserEntity);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("\tDone!");
        }

        await _newContext.SaveChangesAsync();
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
