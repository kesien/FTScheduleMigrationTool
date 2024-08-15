using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MigrationTool.Persistency;

namespace MigrationTool;

public class ApplicationDbContext : IdentityDbContext<UserEntity, UserRoleEntity, Guid>
{
    private readonly IDataProviderSelector _dataProviderSelector;

    public ApplicationDbContext(IDataProviderSelector dataProviderSelector)
    {
        _dataProviderSelector = dataProviderSelector;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<UserEntity>().HasKey(x => x.Id);
        builder.Entity<UserRoleEntity>().HasKey(x => x.Id);
        builder.Entity<LocationEntity>().HasKey(x => x.Id);
        builder.Entity<SeatEntity>().HasKey(x => x.Id);
        builder.Entity<ToDoEntity>().HasKey(x => x.Id);
        builder.Entity<WorkAssignmentEntity>().HasKey(x => x.Id);
        builder.Entity<WorkScheduleEntity>().HasKey(x => x.Id);
        builder.Entity<AbsenceEntity>().HasKey(x => x.Id);
        builder.Entity<RequestEntity>().HasKey(x => x.Id);
        builder.Entity<ApplicationLanguageEntity>().HasKey(x => x.Id);
        builder.Entity<DepartmentEntity>().HasKey(x => x.Id);

        #region UserEntity

        builder.Entity<UserEntity>().HasMany(u => u.Requests)
            .WithOne(r => r.User).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<UserEntity>().HasMany(u => u.Absences)
            .WithOne(r => r.User).HasForeignKey(r => r.UserId);
        builder.Entity<UserEntity>().HasMany(u => u.WorkAssignments)
            .WithOne(r => r.User).HasForeignKey(r => r.UserId);
        builder.Entity<UserEntity>().HasMany(u => u.Departments).WithMany(d => d.Users);

        #endregion

        #region LocationEntity

        builder.Entity<LocationEntity>().HasMany(l => l.Requests).WithOne(r => r.Location)
            .HasForeignKey(l => l.LocationId);
        builder.Entity<LocationEntity>().HasMany(l => l.Seats).WithOne(r => r.Location)
            .HasForeignKey(l => l.LocationId);
        builder.Entity<LocationEntity>().HasMany(l => l.WorkSchedules).WithOne(r => r.Location)
            .HasForeignKey(l => l.LocationId);

        #endregion

        #region WorkScheduleEntity

        builder.Entity<WorkScheduleEntity>().HasMany(we => we.WorkAssignments).WithOne(wa => wa.WorkSchedule)
            .HasForeignKey(wa => wa.WorkScheduleId);

        #endregion

        #region WorkAssignmentEntity

        builder.Entity<WorkAssignmentEntity>().HasMany(wa => wa.Todos).WithMany(todo => todo.WorkAssignments);

        #endregion

        #region DepartmentEntity

        builder.Entity<DepartmentEntity>().HasMany(d => d.Users).WithMany(u => u.Departments);

        #endregion

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        _dataProviderSelector.UseSql(optionsBuilder);
    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<UserRoleEntity> Roles { get; set; }
    public DbSet<LocationEntity> Locations { get; set; }
    public DbSet<SeatEntity> Seats { get; set; }
    public DbSet<ToDoEntity> ToDos { get; set; }
    public DbSet<WorkAssignmentEntity> WorkAssignments { get; set; }
    public DbSet<WorkScheduleEntity> WorkSchedules { get; set; }
    public DbSet<AbsenceEntity> Absences { get; set; }
    public DbSet<RequestEntity> Requests { get; set; }
    public DbSet<ApplicationLanguageEntity> ApplicationLanguages { get; set; }
    public DbSet<DepartmentEntity> Departments { get; set; }
}