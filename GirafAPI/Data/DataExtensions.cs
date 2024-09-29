using Microsoft.EntityFrameworkCore;

namespace GirafAPI.Data;


public static class DataExtensions
{
    // Loads a scoped DbContext into memory
    public static async Task MigrateDbAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}