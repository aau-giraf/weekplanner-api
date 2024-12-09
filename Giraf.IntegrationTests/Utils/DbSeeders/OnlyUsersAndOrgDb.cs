using GirafAPI.Data;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

public class OnlyUsersAndOrgDb : DbSeeder
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
    }
}