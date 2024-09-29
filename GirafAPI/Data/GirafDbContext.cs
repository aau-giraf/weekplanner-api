using GirafAPI.Entities.Resources;
using Microsoft.EntityFrameworkCore;

namespace GirafAPI.Data;

// The in-memory representation of the Giraf database
public class GirafDbContext(DbContextOptions<GirafDbContext> options) : DbContext(options)
{
    public DbSet<Citizen> Citizens => Set<Citizen>();
}