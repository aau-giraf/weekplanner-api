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
    public class GradeWithActivitiesSeeder : DbSeeder
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

            // Create a grade
            var grade = new Grade
            {
                Name = "Activities Grade 1",
                OrganizationId = organization.Id,
                Citizens = new List<Citizen>(),
                Activities = new List<Activity>()
            };
            context.Grades.Add(grade);
            context.SaveChanges();

            // Create activities and add them to the citizen
            var activityDate = DateOnly.FromDateTime(DateTime.UtcNow);

            var activity1 = new Activity
            {
                Date = activityDate,
                Name = "Activity 1",
                Description = "GradeWith ActivitySeeder 1",
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(9, 0),
                IsCompleted = false,
                Pictogram = pictogram
            };

            var activity2 = new Activity
            {
                Date = activityDate,
                Name = "Activity 2",
                Description = "grade With Activity Seeder 2",
                StartTime = new TimeOnly(18, 0),
                EndTime = new TimeOnly(19, 0),
                IsCompleted = false,
                Pictogram = pictogram
            };

            grade.Activities.Add(activity1);
            grade.Activities.Add(activity2);
            context.SaveChanges();
        }
    }
}
