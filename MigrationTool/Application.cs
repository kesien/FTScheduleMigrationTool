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
        var users = await _context.Users.Include(u => u.Absences).Include(u => u.Requests).Include(u => u.WorkAssignments).ToListAsync();
        foreach (var user in users)
        {
            Console.WriteLine($"{user.Email}\tAbsences: {user.Absences.Count}\tRequests: {user.Requests.Count}\tWorkAssignments: {user.WorkAssignments.Count}");
        }
    }
}
