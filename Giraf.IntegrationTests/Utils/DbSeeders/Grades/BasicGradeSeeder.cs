using GirafAPI.Data;
using GirafAPI.Entities.Activities;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Grades;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Giraf.IntegrationTests.Utils.DbSeeders
{
    public class BasicGradeSeeder : DbSeeder
    {
        public override void SeedData(DbContext dbContext)
        {
            var context = (GirafDbContext)dbContext;

            // Create an organization
            var organization = new Organization
            {
                Name = "Test Organization",
                Users = new List<GirafUser>(),
                Grades = new List<Grade>(),
                Citizens = new List<Citizen>()
            };
            context.Organizations.Add(organization);
            context.SaveChanges();

            // Create a grade
            var grade = new Grade
            {
                Name = "Test Grade",
                OrganizationId = organization.Id,
                Citizens = new List<Citizen>(),
                Activities = new List<Activity>()
            };
            context.Grades.Add(grade);
            context.SaveChanges();
        }
    }
}
