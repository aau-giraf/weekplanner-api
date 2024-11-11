using GirafAPI.Configuration;
using GirafAPI.Entities.DTOs;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GirafAPI.Entities.Users.DTOs;

namespace GirafAPI.Endpoints
{
    public static class LoginEndpoint
    {
        public static void MapLoginEndpoint(this WebApplication app)
        {
            app.MapPost("/login", async (LoginDTO loginDTO, UserManager<GirafUser> userManager, IOptions<JwtSettings> jwtSettings) =>
            {
                var user = await userManager.FindByNameAsync(loginDTO.Username);
                if (user == null)
                {
                    return Results.BadRequest("Invalid username or password");
                }
                var passwordValid = await userManager.CheckPasswordAsync(user, loginDTO.Password);
                if (!passwordValid)
                {
                    return Results.BadRequest("Invalid username or password");
                }

                var roles = await userManager.GetRolesAsync(user);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id ?? throw new ArgumentNullException(nameof(user.Id))),
                    new Claim(ClaimTypes.Name, user.UserName ?? throw new ArgumentNullException(nameof(user.UserName)))
                };

                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.SecretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: jwtSettings.Value.Issuer,
                    audience: jwtSettings.Value.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: creds);

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return Results.Ok(new { Token = tokenString });
            })
            .WithName("UserLogin")
            .WithTags("Authentication")
            .WithDescription("Authenticates a user and returns a JWT token.")
            .Accepts<LoginDTO>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
}