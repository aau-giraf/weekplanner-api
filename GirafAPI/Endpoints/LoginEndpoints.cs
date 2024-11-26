using GirafAPI.Configuration;
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
            app.MapPost("/login", async (LoginDTO loginDTO, UserManager<GirafUser> userManager, SignInManager<GirafUser> signInManager,  IOptions<JwtSettings> jwtSettings) =>
            {
                var user = await userManager.FindByNameAsync(loginDTO.Username);
                if (user == null)
                {
                    return Results.BadRequest("Invalid username or password");
                }
                var signIn = await signInManager.PasswordSignInAsync(user, 
                                                                               loginDTO.Password, 
                                                                               isPersistent: false, 
                                                                               lockoutOnFailure: false);
                if (!signIn.Succeeded)
                {
                    return Results.BadRequest("Invalid username or password");
                }

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName)
                };

                var userClaims = await userManager.GetClaimsAsync(user);
                claims.AddRange(userClaims);

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
