using GirafAPI.Endpoints;
using GirafAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.ConfigureDatabase(builder.Configuration, builder.Environment)
    .ConfigureIdentity()
    .ConfigureJwt(builder.Configuration)
    .ConfigureAuthorizationPolicies()
    .ConfigureSwagger();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapCitizensEndpoints();
app.MapUsersEndpoints();
app.MapLoginEndpoint();

// Apply migrations, also contains seed data, but not needed
await app.ApplyMigrationsAsync();

app.Run();