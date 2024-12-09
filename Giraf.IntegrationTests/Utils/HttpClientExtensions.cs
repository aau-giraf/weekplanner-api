using System.Net.Http.Headers;
using System.Security.Claims;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Giraf.IntegrationTests.Utils;

public static class HttpClientExtensions
{
    public static void AttachClaimsToken(this HttpClient httpClient, IServiceScope scope, GirafUser user)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>();
        var claims = userManager.GetClaimsAsync(user).GetAwaiter().GetResult().ToList();
        
        var token = new TestJwtToken();
        var tokenString = token.Build(claims);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);
    }
}