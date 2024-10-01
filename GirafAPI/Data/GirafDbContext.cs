using GirafAPI.Entities.Resources;
using GirafAPI.Entities.Weekplans;
using Microsoft.EntityFrameworkCore;

namespace GirafAPI.Data;

// The in-memory representation of the Giraf database
public class GirafDbContext(DbContextOptions<GirafDbContext> options) : DbContext(options)
{
    public DbSet<Citizen> Citizens => Set<Citizen>();
    public DbSet<Weekplan> Weekplans => Set<Weekplan>();
}