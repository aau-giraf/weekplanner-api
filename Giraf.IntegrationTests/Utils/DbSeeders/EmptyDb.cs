using GirafAPI.Data;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

public class EmptyDb : DbSeeder
{
    public override void SeedData(GirafDbContext dbContext, UserManager<GirafUser> userManager)
    {
    }
}