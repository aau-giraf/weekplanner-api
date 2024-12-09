using GirafAPI.Data;
using GirafAPI.Entities.Invitations;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

public class BaseCaseDb : DbSeeder
{
    public override void SeedData(GirafDbContext dbContext, UserManager<GirafUser> userManager)
    {
        SeedUsers(userManager);
        SeedOrganization(
            dbContext,
            userManager,
            Users["owner"],
            [Users["admin"]],
            [Users["member"]]);
        SeedCitizens(dbContext, Organizations.First());
        SeedPictogram(dbContext, Organizations.First());
        SeedCitizenActivity(dbContext, Citizens.First().Id, Pictograms.First());
        SeedGrade(dbContext, Organizations.First());
        SeedGradeActivity(dbContext, Grades.First().Id, Pictograms.First());
    }
}