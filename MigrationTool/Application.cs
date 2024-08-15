using Microsoft.EntityFrameworkCore;

namespace MigrationTool;

public class Application
{
    private readonly ApplicationDbContext _context;

    public Application(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task Test()
    {
        var schedules = await _context.WorkSchedules.ToListAsync();
        foreach (var workScheduleEntity in schedules)
        {
            Console.WriteLine(workScheduleEntity.StartDate);
        }
    }
}
