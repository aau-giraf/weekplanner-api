using GirafAPI.Data;
using GirafAPI.Endpoints;
using GirafAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.ConfigureDatabase(builder.Configuration, builder.Environment)
    .ConfigureIdentity()
    .ConfigureJwt(builder.Configuration)
    .ConfigureAuthorizationPolicies()
    .ConfigureSwagger();

builder.Services.AddAntiforgery(options =>
    {
      options.Cookie.Expiration = TimeSpan.Zero;
    });

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Map endpoints
app.MapCitizensEndpoints();
app.MapUsersEndpoints();
app.MapLoginEndpoint();
app.MapActivityEndpoints();
app.MapOrganizationEndpoints();
app.MapInvitationEndpoints();
app.MapPictogramEndpoints();


await app.ApplyMigrationsAsync();
await app.SeedDataAsync();

if (app.Environment.IsDevelopment())
{
    app.Run("http://0.0.0.0:5171");
}
else
{
    app.Run();
}

