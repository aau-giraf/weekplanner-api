using Microsoft.EntityFrameworkCore;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

public class EmptyDb : DbSeeder
{
    public EmptyDb()
    {
    }
    public override void SeedData(DbContext context)
    {
    }
}