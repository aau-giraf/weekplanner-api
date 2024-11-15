using Microsoft.EntityFrameworkCore;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

public abstract class DbSeeder
{
    public void Seed(DbContext context)
    {
        

        SeedData(context);
    }

    public abstract void SeedData(DbContext context);
}