using GirafAPI.Entities.Resources;
using Microsoft.EntityFrameworkCore;

namespace GirafAPI.Data;

public class GirafDbContext(DbContextOptions<GirafDbContext> options) : DbContext(options)
{
    public DbSet<Citizen> Citizens => Set<Citizen>();
}