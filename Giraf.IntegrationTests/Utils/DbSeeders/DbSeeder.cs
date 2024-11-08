using Microsoft.EntityFrameworkCore;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

public abstract class DbSeeder
{
    public void Seed(DbContext context)
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        SeedData(context);
    }

    public abstract void SeedData(DbContext context);
}