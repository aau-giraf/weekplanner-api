using GirafAPI.Data;
using GirafAPI.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Use Sqlite for development environment
if (builder.Environment.IsDevelopment())
{
    var connString = builder.Configuration.GetConnectionString("GirafDb");
    builder.Services.AddSqlite<GirafDbContext>(connString);
}

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapCitizensEndpoints();

await app.MigrateDbAsync();

app.Run();