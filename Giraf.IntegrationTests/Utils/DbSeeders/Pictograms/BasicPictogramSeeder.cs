using GirafAPI.Data;
using GirafAPI.Entities.Pictograms;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;
using Microsoft.EntityFrameworkCore;
using GirafAPI.Entities.Grades;
using GirafAPI.Entities.Citizens;

namespace Giraf.IntegrationTests.Utils.DbSeeders
{
    public class BasicPictogramSeeder : DbSeeder
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

            // Create pictograms
            var pictogram1 = new Pictogram
            {
                PictogramName = "Pictogram 1",
                PictogramUrl = "http://example.com/pictogram1.png",
                OrganizationId = organization.Id
            };

            var pictogram2 = new Pictogram
            {
                PictogramName = "Pictogram 2",
                PictogramUrl = "http://example.com/pictogram2.png",
                OrganizationId = organization.Id
            };

            context.Pictograms.AddRange(pictogram1, pictogram2);
            context.SaveChanges();
        }
    }
}
