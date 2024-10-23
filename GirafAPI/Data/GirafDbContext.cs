using GirafAPI.Entities.Resources;
using GirafAPI.Entities.Weekplans;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using GirafAPI.Entities.Users;
using GirafAPI.Entities.Organizations;

namespace GirafAPI.Data
{
    public class GirafDbContext : IdentityDbContext<GirafUser>
    {
        public GirafDbContext(DbContextOptions<GirafDbContext> options) : base(options)
        {
        }

        public DbSet<Citizen> Citizens => Set<Citizen>();
        public DbSet<Activity> Activities => Set<Activity>();
        public DbSet<Organization> Organizations { get; set; }
    }
}