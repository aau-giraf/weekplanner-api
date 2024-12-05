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
    public class ActivityAndPictogramSeeder : DbSeeder
    {
        public override void SeedData(DbContext dbContext)
        {
            var context = (GirafDbContext)dbContext;

            // Create an organization
            var organization = new Organization
            {
                Name = "Activity and Pictogram Seeder Test Organization",
                Users = new List<GirafUser>(),
                Grades = new List<Grade>(),
                Citizens = new List<Citizen>()
            };
            context.Organizations.Add(organization);
            context.SaveChanges();

            // Create pictograms
            var initialPictogram = new Pictogram
            {
                PictogramName = "Initial Test Pictogram",
                PictogramUrl = "http://example.com/initial.png",
                OrganizationId = organization.Id
            };
            var pictogramToAssign = new Pictogram
            {
                PictogramName = "Test Pictogram to Assign",
                PictogramUrl = "http://example.com/assign.png",
                OrganizationId = organization.Id
            };
            context.Pictograms.AddRange(initialPictogram, pictogramToAssign);
            context.SaveChanges();

            // Create a citizen
            var citizen = new Citizen
            {
                FirstName = "Thomas",
                LastName = "Thomsen",
                Organization = organization,
                Activities = new List<Activity>()
            };
            context.Citizens.Add(citizen);
            context.SaveChanges();

            // Create an activity with the initial pictogram
            var activity = new Activity
            {
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(11, 0),
                IsCompleted = false,
                Pictogram = initialPictogram
            };

            // Add activity to the citizen
            citizen.Activities.Add(activity);
            context.SaveChanges();
        }
    }
}
