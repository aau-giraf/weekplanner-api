using GirafAPI.Data;
using GirafAPI.Entities.Activities;
using GirafAPI.Entities.Pictograms;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;
using Microsoft.EntityFrameworkCore;
using GirafAPI.Entities.Grades;

namespace Giraf.IntegrationTests.Utils.DbSeeders
{
    public class CitizenWithActivitiesSeeder : DbSeeder
    {
        public override void SeedData(DbContext dbContext)
        {
            var context = (GirafDbContext)dbContext;

            // Create an organization
            var organization = new Organization
            {
                Name = "CitizenWithActivity Test Organization",
                Users = new List<GirafUser>(),
                Grades = new List<Grade>(),
                Citizens = new List<Citizen>()
            };
            context.Organizations.Add(organization);
            context.SaveChanges();

            // Create a pictogram
            var pictogram = new Pictogram
            {
                PictogramName = "CitzenWithActivity Seeder Sample Pictogram",
                PictogramUrl = "http://example.com/pictogram.png",
                OrganizationId = organization.Id
            };
            context.Pictograms.Add(pictogram);
            context.SaveChanges();

            // Create a citizen
            var citizen = new Citizen
            {
                FirstName = "CitizenWith",
                LastName = "ActivitySeeder",
                Organization = organization,
                Activities = new List<Activity>()
            };
            context.Citizens.Add(citizen);
            context.SaveChanges();

            // Create activities and add them to the citizen
            var activityDate = DateOnly.FromDateTime(DateTime.UtcNow);

            var activity1 = new Activity
            {
                Date = activityDate,
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(9, 0),
                IsCompleted = false,
                Pictogram = pictogram
            };

            var activity2 = new Activity
            {
                Date = activityDate,
                StartTime = new TimeOnly(18, 0),
                EndTime = new TimeOnly(19, 0),
                IsCompleted = false,
                Pictogram = pictogram
            };

            citizen.Activities.Add(activity1);
            citizen.Activities.Add(activity2);
            context.SaveChanges();
        }
    }
}
