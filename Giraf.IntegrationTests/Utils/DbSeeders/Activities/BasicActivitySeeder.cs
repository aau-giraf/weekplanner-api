using GirafAPI.Data;
using GirafAPI.Entities.Activities;
using GirafAPI.Entities.Pictograms;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Users;
using Microsoft.EntityFrameworkCore;
using GirafAPI.Entities.Grades;

namespace Giraf.IntegrationTests.Utils.DbSeeders
{
    public class BasicActivitySeeder : DbSeeder
    {
        public override void SeedData(DbContext dbContext)
        {
            var context = (GirafDbContext)dbContext;

            // Create an organization
            var organization = new Organization
            {
                Name = "Basic Activity Test Organization",
                Users = new List<GirafUser>(),
                Grades = new List<Grade>(),
                Citizens = new List<Citizen>()
            };
            context.Organizations.Add(organization);
            context.SaveChanges();

            // Create a pictogram
            var pictogram = new Pictogram
            {
                PictogramName = "Basic Activity Pictogram",
                PictogramUrl = "http://example.com/pictogram.png",
                OrganizationId = organization.Id
            };
            context.Pictograms.Add(pictogram);
            context.SaveChanges();

            // Create a citizen associated with the organization
            var citizen = new Citizen
            {
                FirstName = "BasicActivity",
                LastName = "Seeder",
                Organization = organization,
                Activities = new List<Activity>()
            };
            context.Citizens.Add(citizen);
            context.SaveChanges();

            // Create an activity associated with the citizen
            var activity = new Activity
            {
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                Name = "Sample Activity",
                Description = "This is a sample activity for testing.",
                StartTime = TimeOnly.FromDateTime(DateTime.UtcNow),
                EndTime = TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)),
                IsCompleted = false,
                Pictogram = pictogram
            };

            // Add activity to the citizen
            citizen.Activities.Add(activity);
            context.SaveChanges();
        }
    }
}
