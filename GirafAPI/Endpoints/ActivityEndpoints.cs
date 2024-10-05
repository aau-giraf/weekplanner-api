namespace GirafAPI.Endpoints;

public static class ActivityEndpoints
{
    public static RouteGroupBuilder MapActivityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("activity");

        return group;
    }
}