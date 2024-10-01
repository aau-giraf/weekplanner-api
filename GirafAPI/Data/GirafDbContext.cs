using GirafAPI.Entities.Resources;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GirafAPI.Data
{
    public class GirafDbContext : IdentityDbContext<GirafUser>
    {
        public GirafDbContext(DbContextOptions<GirafDbContext> options) : base(options)
        {
        }

        public DbSet<Citizen> Citizens => Set<Citizen>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure the one-to-one relationship between Citizen and GirafUser
            builder.Entity<Citizen>()
                .HasOne(c => c.User)
                .WithOne()
                .HasForeignKey<Citizen>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}