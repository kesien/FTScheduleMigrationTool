using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MigrationTool.Persistency;

namespace MigrationTool;

public class NewApplicationDbContext : IdentityDbContext<UserEntity, UserRoleEntity, Guid>
{
    public NewApplicationDbContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<NewUserEntity>().HasKey(x => x.Id);
        builder.Entity<CalendarEntity>().HasKey(x => x.Id);
        builder.Entity<EventEntity>().HasKey(x => x.Id);
        builder.Entity<RecurrenceEntity>().HasKey(x => x.Id);
        builder.Entity<RecurrenceExceptionEntity>().HasKey(x => x.Id);
        builder.Entity<UserRoleEntity>().HasKey(x => x.Id);
        builder.Entity<LocationEntity>().HasKey(x => x.Id);
        builder.Entity<SeatEntity>().HasKey(x => x.Id);
        builder.Entity<ToDoEntity>().HasKey(x => x.Id);
        builder.Entity<ApplicationLanguageEntity>().HasKey(x => x.Id);
        builder.Entity<DepartmentEntity>().HasKey(x => x.Id);

        #region UserEntity

        builder.Entity<NewUserEntity>().HasMany(u => u.Departments).WithMany(d => d.Users);
        builder.Entity<NewUserEntity>().HasOne(u => u.Calendar);

        #endregion

        #region CalendarEntity

        builder.Entity<CalendarEntity>().HasMany(c => c.Events).WithOne(e => e.Calendar);

        #endregion

        #region EventEntity

        builder.Entity<EventEntity>().HasMany(e => e.Recurrences).WithOne(r => r.Event);
        builder.Entity<EventEntity>().HasMany(e => e.Exceptions).WithOne(e => e.Event);

        #endregion

        #region LocationEntity

        builder.Entity<LocationEntity>().HasMany(l => l.Seats).WithOne(r => r.Location)
            .HasForeignKey(l => l.LocationId);

        #endregion

        #region DepartmentEntity

        builder.Entity<DepartmentEntity>().HasMany(d => d.Users).WithMany(u => u.Departments);

        #endregion

    }

    public DbSet<NewUserEntity> Users { get; set; }
    public DbSet<CalendarEntity> Calendars { get; set; }
    public DbSet<EventEntity> Events { get; set; }
    public DbSet<RecurrenceEntity> Recurrences { get; set; }
    public DbSet<RecurrenceExceptionEntity> RecurrenceExceptions { get; set; }
    public DbSet<UserRoleEntity> Roles { get; set; }
    public DbSet<LocationEntity> Locations { get; set; }
    public DbSet<SeatEntity> Seats { get; set; }
    public DbSet<ToDoEntity> ToDos { get; set; }
    public DbSet<ApplicationLanguageEntity> ApplicationLanguages { get; set; }
    public DbSet<DepartmentEntity> Departments { get; set; }
}