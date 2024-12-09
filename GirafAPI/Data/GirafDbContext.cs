using GirafAPI.Entities.Grades;
using GirafAPI.Entities.Invitations;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Activities;
using GirafAPI.Entities.Pictograms;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using GirafAPI.Entities.Users;

namespace GirafAPI.Data
{
    public class GirafDbContext : IdentityDbContext<GirafUser>
    {
        public GirafDbContext(DbContextOptions<GirafDbContext> options) : base(options)
        {
        }

        public DbSet<Citizen> Citizens { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Pictogram> Pictograms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 

            modelBuilder.Entity<Citizen>()
                .HasMany(c => c.Activities)        
                .WithOne()                          
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Grade>()
                .HasMany(g => g.Activities)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Pictogram>()
                .HasMany<Activity>()  
                .WithOne(a => a.Pictogram)           
                .OnDelete(DeleteBehavior.SetNull); 
        }
    }
}