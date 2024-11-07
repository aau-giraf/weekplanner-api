using GirafAPI.Entities.Grades;
using GirafAPI.Entities.Invitations;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Weekplans;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using GirafAPI.Entities.Users;

namespace GirafAPI.Data
{
    public class GirafDbContext : IdentityDbContext<GirafUser>
    {
        public GirafDbContext(DbContextOptions<GirafDbContext> options) : base(options)
        {}

        public DbSet<Citizen> Citizens => Set<Citizen>();
        public DbSet<Activity> Activities => Set<Activity>();
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<Invitation> Invitations => Set<Invitation>();
        public DbSet<Grade> Grades { get; set; }
    }
}