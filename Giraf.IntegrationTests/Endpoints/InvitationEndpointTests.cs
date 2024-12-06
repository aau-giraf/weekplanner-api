using Giraf.IntegrationTests.Utils;
using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Data;
using GirafAPI.Entities.Invitations.DTOs;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;


namespace Giraf.IntegrationTests.Endpoints
{
    [Collection("IntegrationTests")]
    public class InvitationEndpointsTests
    {
        #region Get Invitation by ID Tests - Test 1-4
        //1. Tests if you can succesfully get an invitation with no errors
        [Fact]
        public async Task GetInvitationById_ReturnsInvitation_WhenInvitationExists()
        {
            // Arrange
            var originalClaims = TestAuthHandler.TestClaims.ToList();
            TestAuthHandler.TestClaims.Clear();
            
            try
            {
                var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
                using var scope = factory.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();

                // Retrieve an existing invitation from the seeded data
                var existingInvitation = await dbContext.Invitations.FirstOrDefaultAsync();
                Assert.NotNull(existingInvitation);

                // Set the user claims so that the user is the invitation's receiver
                // This must be done BEFORE creating the client so that the authenticated user is correct.
                TestAuthHandler.TestClaims.Clear();
                TestAuthHandler.TestClaims.Add(new Claim(ClaimTypes.NameIdentifier, existingInvitation.ReceiverId));

                // Now create the client, which will use the claims set above
                var client = factory.CreateClient();

                // Act
                var response = await client.GetAsync($"/invitations/{existingInvitation.Id}");

                // Assert
                response.EnsureSuccessStatusCode();
            }
            finally
            {
                // Restore original claims for other tests
                TestAuthHandler.TestClaims.Clear();
                TestAuthHandler.TestClaims.AddRange(originalClaims);
            }
        }

        //2. Tests if you get a Not Found if invitation doesn't exist
        [Fact] 
        public async Task GetInvitationById_ReturnsNotFound_WhenInvitationDoesnotExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(_ => new EmptyDb());
            var client = factory.CreateClient();
            var fakeId = 123;

            // Act
            var response = await client.GetAsync($"/invitations/{fakeId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        //3. Tests if you get a Not Found if sender doesn't exist
        [Fact]
        public async Task GetInvitationById_ReturnsNotFound_WhenSenderDoesnotExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
            
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var existingInvitation = await dbContext.Invitations.FirstAsync();
            Assert.NotNull(existingInvitation);

            // Modify the invitation so that sender doesn't exist
            existingInvitation.SenderId = "";
            await dbContext.SaveChangesAsync();

            // To pass authorization and reach the endpoint code, set user as receiver
            TestAuthHandler.TestClaims.Clear();
            TestAuthHandler.TestClaims.Add(new Claim(ClaimTypes.NameIdentifier, existingInvitation.ReceiverId));

            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync($"/invitations/{existingInvitation.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        //4. Tests if you get a Not Found if organization doesn't exist
        [Fact]
        public async Task GetInvitationById_ReturnsNotFound_WhenOrganizationDoesnotExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
            
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var existingInvitation = await dbContext.Invitations.FirstAsync();
            Assert.NotNull(existingInvitation);

            // Make the organization non-existent
            existingInvitation.OrganizationId = 321;
            await dbContext.SaveChangesAsync();

            // Set user as the receiver to pass authorization
            TestAuthHandler.TestClaims.Clear();
            TestAuthHandler.TestClaims.Add(new Claim(ClaimTypes.NameIdentifier, existingInvitation.ReceiverId));

            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync($"/invitations/{existingInvitation.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion
        
        #region Get Invitation by User ID Tests - Test 5-8
        //5. Tests if you can successfully get an invitation with the receiver's id and get no errors
        [Fact] 
        public async Task GetUserInvitation_ReturnsInvitation_WhenInvitationExists()
        {
            var originalClaims = TestAuthHandler.TestClaims.ToList();
            TestAuthHandler.TestClaims.Clear();

            try
            {
                var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));

                using var scope = factory.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var existingRecievingUser = await dbContext.Users.FirstOrDefaultAsync();
                Assert.NotNull(existingRecievingUser);

                // Authorize the request as the receiving user
                TestAuthHandler.TestClaims.Add(new Claim(ClaimTypes.NameIdentifier, existingRecievingUser.Id));
                var client = factory.CreateClient();

                // Act
                var response = await client.GetAsync($"/invitations/user/{existingRecievingUser.Id}");

                // Assert
                response.EnsureSuccessStatusCode();
                // Since an invitation exists, verify we get a non-empty array
                var invitationDtos = await response.Content.ReadFromJsonAsync<IEnumerable<InvitationDTO>>();
                Assert.NotNull(invitationDtos);
                Assert.NotEmpty(invitationDtos);
            }
            finally
            {
                TestAuthHandler.TestClaims.Clear();
                TestAuthHandler.TestClaims.AddRange(originalClaims);
            }
        }

        //6. Tests if you get an empty array if user doesn't have an invitation
        [Fact] 
        public async Task GetUserInvitation_ReturnsEmptyArray_WhenNoInvitationExists()
        {
            var originalClaims = TestAuthHandler.TestClaims.ToList();
            TestAuthHandler.TestClaims.Clear();

            try
            {
                var factory = new GirafWebApplicationFactory(sp => new BasicUserSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
                
                var fakeId = "123";
                TestAuthHandler.TestClaims.Add(new Claim(ClaimTypes.NameIdentifier, fakeId));
                var client = factory.CreateClient();

                // Act
                var response = await client.GetAsync($"/invitations/user/{fakeId}");

                // Assert
                response.EnsureSuccessStatusCode();
                var invitationDtos = await response.Content.ReadFromJsonAsync<IEnumerable<InvitationDTO>>();
                Assert.NotNull(invitationDtos);
                Assert.Empty(invitationDtos);
            }
            finally
            {
                TestAuthHandler.TestClaims.Clear();
                TestAuthHandler.TestClaims.AddRange(originalClaims);
            }
        }

        //7. Tests if you get an empty array if invitation is found but sender is null
        [Fact] 
        public async Task GetUserInvitation_ReturnsEmptyArray_WhenInvitationExistsButSenderIsNull()
        {
            var originalClaims = TestAuthHandler.TestClaims.ToList();
            TestAuthHandler.TestClaims.Clear();

            try
            {
                var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
                var client = factory.CreateClient();

                using var scope = factory.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var existingRecievingUser = await dbContext.Users.FirstOrDefaultAsync();
                Assert.NotNull(existingRecievingUser);

                var existingInvitation = await dbContext.Invitations.FirstOrDefaultAsync();
                Assert.NotNull(existingInvitation);
                // Make the sender null
                existingInvitation.SenderId = "";
                await dbContext.SaveChangesAsync();

                // Authorize as the receiving user
                TestAuthHandler.TestClaims.Add(new Claim(ClaimTypes.NameIdentifier, existingRecievingUser.Id));

                // Act
                var response = await client.GetAsync($"/invitations/user/{existingRecievingUser.Id}");

                // Assert
                response.EnsureSuccessStatusCode();
                // No valid invitations found after filtering sender-less ones
                var invitationDtos = await response.Content.ReadFromJsonAsync<IEnumerable<InvitationDTO>>();
                Assert.NotNull(invitationDtos);
                Assert.Empty(invitationDtos);
            }
            finally
            {
                TestAuthHandler.TestClaims.Clear();
                TestAuthHandler.TestClaims.AddRange(originalClaims);
            }
        }

        //8. Tests if you get an empty array if invitation is found but organization is null
        [Fact] 
        public async Task GetUserInvitation_ReturnsEmptyArray_WhenInvitationExistsButOrganizationIsNull()
        {
            var originalClaims = TestAuthHandler.TestClaims.ToList();
            TestAuthHandler.TestClaims.Clear();

            try
            {
                var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
                var client = factory.CreateClient();

                using var scope = factory.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var existingRecievingUser = await dbContext.Users.FirstOrDefaultAsync();
                Assert.NotNull(existingRecievingUser);

                var existingInvitation = await dbContext.Invitations.FirstOrDefaultAsync();
                Assert.NotNull(existingInvitation);
                // Make organization invalid
                existingInvitation.OrganizationId = 321;
                await dbContext.SaveChangesAsync();

                // Authorize as the receiving user
                TestAuthHandler.TestClaims.Add(new Claim(ClaimTypes.NameIdentifier, existingRecievingUser.Id));

                // Act
                var response = await client.GetAsync($"/invitations/user/{existingRecievingUser.Id}");

                // Assert
                response.EnsureSuccessStatusCode();
                // Invitation is filtered out since organization doesn't exist, resulting in empty array
                var invitationDtos = await response.Content.ReadFromJsonAsync<IEnumerable<InvitationDTO>>();
                Assert.NotNull(invitationDtos);
                Assert.Empty(invitationDtos);
            }
            finally
            {
                TestAuthHandler.TestClaims.Clear();
                TestAuthHandler.TestClaims.AddRange(originalClaims);
            }
        }
        #endregion

        #region Get Invitation by Organization ID - Tests 9-11

        //9. Tests if you can successfully get an invitation with the organization's id and get no errors
        [Fact]
        public async Task GetOrganizationInvitation_ReturnsInvitation_WhenInvitationExists()
        {
            var originalClaims = TestAuthHandler.TestClaims.ToList();
            TestAuthHandler.TestClaims.Clear();

            try
            {
                // Arrange
                var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
                using var scope = factory.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                
                var existingOrganization = await dbContext.Organizations.FirstOrDefaultAsync();
                Assert.NotNull(existingOrganization);

                // Add OrgAdmin claim to pass "OrganizationAdmin" policy
                TestAuthHandler.TestClaims.Add(new Claim("OrgAdmin", existingOrganization.Id.ToString()));

                var client = factory.CreateClient();

                // Act
                var response = await client.GetAsync($"/invitations/org/{existingOrganization.Id}");

                // Assert
                response.EnsureSuccessStatusCode();
            }
            finally
            {
                TestAuthHandler.TestClaims.Clear();
                TestAuthHandler.TestClaims.AddRange(originalClaims);
            }
        }

        //10. Tests if you get a Not Found if invitation doesn't match organization Id
        [Fact]
        public async Task GetOrganizationInvitation_ReturnsNotFound_WhenNoInvitationWithValidOrganizationIdExists()
        {
            var originalClaims = TestAuthHandler.TestClaims.ToList();
            TestAuthHandler.TestClaims.Clear();

            try
            {
                // Arrange
                var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
                using var scope = factory.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                
                var existingOrganization = await dbContext.Organizations.FirstOrDefaultAsync();
                Assert.NotNull(existingOrganization);

                var existingInvitation = await dbContext.Invitations.FirstOrDefaultAsync();
                Assert.NotNull(existingInvitation);

                // Change the invitation's organization to one that doesn't match
                existingInvitation.OrganizationId = 321;
                dbContext.SaveChanges();

                // Add OrgAdmin claim for the original organization
                TestAuthHandler.TestClaims.Add(new Claim("OrgAdmin", existingOrganization.Id.ToString()));

                var client = factory.CreateClient();

                // Act
                var response = await client.GetAsync($"/invitations/org/{existingOrganization.Id}");

                // Assert
                // Since no matching invitation for the given orgId now, expect NotFound
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
            finally
            {
                TestAuthHandler.TestClaims.Clear();
                TestAuthHandler.TestClaims.AddRange(originalClaims);
            }
        }

        //11. Tests if you get a Not Found if invitation doesn't have a valid sender
        [Fact]
        public async Task GetOrganizationInvitation_ReturnsNotFound_WhenNoValidSenderExists()
        {
            var originalClaims = TestAuthHandler.TestClaims.ToList();
            TestAuthHandler.TestClaims.Clear();

            try
            {
                // Arrange
                var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
                using var scope = factory.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                
                var existingOrganization = await dbContext.Organizations.FirstOrDefaultAsync();
                Assert.NotNull(existingOrganization);

                var existingInvitation = await dbContext.Invitations.FirstOrDefaultAsync();
                Assert.NotNull(existingInvitation);

                // Invalidate the sender
                existingInvitation.SenderId = "";
                dbContext.SaveChanges();

                // Add OrgAdmin claim for the organization
                TestAuthHandler.TestClaims.Add(new Claim("OrgAdmin", existingOrganization.Id.ToString()));

                var client = factory.CreateClient();

                // Act
                var response = await client.GetAsync($"/invitations/org/{existingOrganization.Id}");

                // Assert
                // Since the sender is invalid, endpoint returns NotFound
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
            finally
            {
                TestAuthHandler.TestClaims.Clear();
                TestAuthHandler.TestClaims.AddRange(originalClaims);
            }
        }
        
        #endregion
    
        #region Post Invitation - Tests 12-13 

        //12. Successfully posts a new invitation
        [Fact]
        public async Task PostInvitation_ReturnsCreated_IfSucessfullyCreated()
        {
            var originalClaims = TestAuthHandler.TestClaims.ToList();
            TestAuthHandler.TestClaims.Clear();

            try
            {
                // Arrange
                var factory = new GirafWebApplicationFactory(sp => new UserWithOrganizationsSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
                
                using var scope = factory.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>();

                var sender = await dbContext.Users.FirstOrDefaultAsync();
                Assert.NotNull(sender);

                var receiver = new GirafUser {
                    UserName = "RecieverUser",
                    Email = "RecieverUser@example.com",
                    FirstName = "RecieverUser",
                    LastName = "User",
                    Organizations = new List<Organization>()
                };
                var createResult = await userManager.CreateAsync(receiver, "ReceiverPassword123!");
                Assert.True(createResult.Succeeded);

                var existingOrganization = await dbContext.Organizations.FirstOrDefaultAsync();
                Assert.NotNull(existingOrganization);

                var newInvitationDto = new CreateInvitationDTO(existingOrganization.Id, receiver.Email, sender.Id);

                // Authorize as OrgMember
                TestAuthHandler.TestClaims.Add(new Claim(ClaimTypes.NameIdentifier, sender.Id));
                TestAuthHandler.TestClaims.Add(new Claim("OrgMember", existingOrganization.Id.ToString()));

                var client = factory.CreateClient();

                // Act
                var response = await client.PostAsJsonAsync("/invitations/", newInvitationDto);

                // Assert
                response.EnsureSuccessStatusCode();
            }
            finally
            {
                TestAuthHandler.TestClaims.Clear();
                TestAuthHandler.TestClaims.AddRange(originalClaims);
            }
        }


        //13. Throws bad request if receiver is not found
        [Fact]
        public async Task PostInvitation_ReturnsBadRequest_IfRecieverNotFound()
        {
            var originalClaims = TestAuthHandler.TestClaims.ToList();
            TestAuthHandler.TestClaims.Clear();

            try
            {
                // Arrange
                var factory = new GirafWebApplicationFactory(sp => new OrganizationWithUserSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
                using var scope = factory.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>();

                var sender = await dbContext.Users.FirstOrDefaultAsync();
                Assert.NotNull(sender);

                var existingOrganization = await dbContext.Organizations.FirstOrDefaultAsync();
                Assert.NotNull(existingOrganization);

                var fakeRecieverEmail = "fake@email.com";
                var newInvitationDto = new CreateInvitationDTO(existingOrganization.Id, fakeRecieverEmail, sender.Id);

                // Authorize as OrgMember
                TestAuthHandler.TestClaims.Add(new Claim(ClaimTypes.NameIdentifier, sender.Id));
                TestAuthHandler.TestClaims.Add(new Claim("OrgMember", existingOrganization.Id.ToString()));

                var client = factory.CreateClient();

                // Act
                var response = await client.PostAsJsonAsync("/invitations/", newInvitationDto);

                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
            finally
            {
                TestAuthHandler.TestClaims.Clear();
                TestAuthHandler.TestClaims.AddRange(originalClaims);
            }
        }


        #endregion
    
        #region Put Invitation by ID and response - Test 14-18
        
        //14. Tests if you can succesfully accept an invitaion
        [Fact]
        public async Task PutInvitationById_ReturnsOk_WhenInvitationIsAccepted()
        {
            var originalClaims = TestAuthHandler.TestClaims.ToList();
            TestAuthHandler.TestClaims.Clear();

            try
            {
                // Arrange
                var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
                
                using var scope = factory.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var existingInvitation = await dbContext.Invitations.FirstOrDefaultAsync();
                Assert.NotNull(existingInvitation);

                // RespondInvitation requires the user to be the receiver
                TestAuthHandler.TestClaims.Add(new Claim(ClaimTypes.NameIdentifier, existingInvitation.ReceiverId));

                var client = factory.CreateClient();
                var responseDto = new InvitationResponseDTO { Response = true };

                // Act
                var response = await client.PutAsJsonAsync($"/invitations/respond/{existingInvitation.Id}", responseDto);

                // Assert
                response.EnsureSuccessStatusCode();
                dbContext.Entry(existingInvitation).State = EntityState.Detached;
                var deletedInvitation = await dbContext.Invitations.FindAsync(existingInvitation.Id);
                Assert.Null(deletedInvitation);
            }
            finally
            {
                TestAuthHandler.TestClaims.Clear();
                TestAuthHandler.TestClaims.AddRange(originalClaims);
            }
        }

        //15. Tests if you can succesfully decline an invitaion
        [Fact]
        public async Task PutInvitationById_ReturnsOk_WhenInvitationIsDeclined()
        {
            var originalClaims = TestAuthHandler.TestClaims.ToList();
            TestAuthHandler.TestClaims.Clear();

            try
            {
                // Arrange
                var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
                
                using var scope = factory.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var existingInvitation = await dbContext.Invitations.FirstOrDefaultAsync();
                Assert.NotNull(existingInvitation);

                // Be the receiver
                TestAuthHandler.TestClaims.Add(new Claim(ClaimTypes.NameIdentifier, existingInvitation.ReceiverId));

                var client = factory.CreateClient();
                var responseDto = new InvitationResponseDTO { Response = false };

                // Act
                var response = await client.PutAsJsonAsync($"/invitations/respond/{existingInvitation.Id}", responseDto);

                // Assert
                response.EnsureSuccessStatusCode();
                dbContext.Entry(existingInvitation).State = EntityState.Detached;
                var deletedInvitation = await dbContext.Invitations.FindAsync(existingInvitation.Id);
                Assert.Null(deletedInvitation);
            }
            finally
            {
                TestAuthHandler.TestClaims.Clear();
                TestAuthHandler.TestClaims.AddRange(originalClaims);
            }
        }

        //16. Test that makes sure a non-existing invitation Id returns Bad Request.
        [Fact]
        public async Task PutInvitationById_ReturnsNotFound_WhenInvitationIdDoesNotExist()
        {
            var originalClaims = TestAuthHandler.TestClaims.ToList();
            TestAuthHandler.TestClaims.Clear();

            try
            {
                var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
                var client = factory.CreateClient();

                var FakeInvitaionId = "fakeInvitation";
                var responseDto = new InvitationResponseDTO { Response = true };

                // RespondInvitation requires the user to be receiver, but since no invitation,
                // just pick any userId. The endpoint might return BadRequest before checking.
                TestAuthHandler.TestClaims.Add(new Claim(ClaimTypes.NameIdentifier, "neglible-user-id"));

                // Act
                var response = await client.PutAsJsonAsync($"/invitations/respond/{FakeInvitaionId}", responseDto);

                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
            finally
            {
                TestAuthHandler.TestClaims.Clear();
                TestAuthHandler.TestClaims.AddRange(originalClaims);
            }
        }

        //17. Test that makes sure that an accepted invitation without an organization returns Not Found
        [Fact]
        public async Task PutInvitationById_ReturnsNotFound_WhenInvitationDoesNotHaveAnOrganization()
        {
            var originalClaims = TestAuthHandler.TestClaims.ToList();
            TestAuthHandler.TestClaims.Clear();

            try
            {
                var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
                
                using var scope = factory.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var existingInvitation = await dbContext.Invitations.FirstOrDefaultAsync();
                Assert.NotNull(existingInvitation);

                existingInvitation.OrganizationId = 1000;
                await dbContext.SaveChangesAsync();

                var responseDto = new InvitationResponseDTO { Response = true };

                // Must be the receiver
                TestAuthHandler.TestClaims.Add(new Claim(ClaimTypes.NameIdentifier, existingInvitation.ReceiverId));

                var client = factory.CreateClient();
                var response = await client.PutAsJsonAsync($"/invitations/respond/{existingInvitation.Id}", responseDto);

                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
            finally
            {
                TestAuthHandler.TestClaims.Clear();
                TestAuthHandler.TestClaims.AddRange(originalClaims);
            }
        }

        //18. Accepted invitation without a receiver returns Forbidden
        [Fact]
        public async Task PutInvitationById_ReturnsNotFound_WhenInvitationDoesNotHaveAReciever()
        {
            var originalClaims = TestAuthHandler.TestClaims.ToList();
            TestAuthHandler.TestClaims.Clear();

            try
            {
                var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
                
                using var scope = factory.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var existingInvitation = await dbContext.Invitations.FirstOrDefaultAsync();
                Assert.NotNull(existingInvitation);

                existingInvitation.ReceiverId = "";
                await dbContext.SaveChangesAsync();

                var responseDto = new InvitationResponseDTO { Response = true };

                TestAuthHandler.TestClaims.Add(new Claim(ClaimTypes.NameIdentifier, "randomUserId"));

                var client = factory.CreateClient();
                var response = await client.PutAsJsonAsync($"/invitations/respond/{existingInvitation.Id}", responseDto);

                Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            }
            finally
            {
                TestAuthHandler.TestClaims.Clear();
                TestAuthHandler.TestClaims.AddRange(originalClaims);
            }
        }
        #endregion

        #region Delete Invitation by ID - Test 18-

        //19. Tests if you can succesfully accept an invitaion
        [Fact]
        public async Task DeleteInvitationById_ReturnsOk_WhenInvitationIsDeleted()
        {
            var originalClaims = TestAuthHandler.TestClaims.ToList();
            TestAuthHandler.TestClaims.Clear();

            try
            {
                var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
                
                using var scope = factory.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var existingInvitation = await dbContext.Invitations.FirstOrDefaultAsync();
                Assert.NotNull(existingInvitation);

                var organizationId = existingInvitation.OrganizationId;

                // OrgAdmin claim required
                TestAuthHandler.TestClaims.Add(new Claim("OrgAdmin", organizationId.ToString()));

                var client = factory.CreateClient();

                // Act
                var response = await client.DeleteAsync($"/invitations/{existingInvitation.Id}");

                // Assert
                response.EnsureSuccessStatusCode();
                dbContext.Entry(existingInvitation).State = EntityState.Detached;

                var deletedInvitation = await dbContext.Invitations.FindAsync(existingInvitation.Id);
                Assert.Null(deletedInvitation);
            }
            finally
            {
                TestAuthHandler.TestClaims.Clear();
                TestAuthHandler.TestClaims.AddRange(originalClaims);
            }
        }

        //20. Returns Bad Request when invitation does not exist (delete)
        [Fact]
        public async Task DeleteInvitationById_ReturnsBadRequest_WhenInvitationIdDoesNotExist()
        {
            var originalClaims = TestAuthHandler.TestClaims.ToList();
            TestAuthHandler.TestClaims.Clear();

            try
            {
                var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
                
                // Need an OrgAdmin claim for any org. Just pick "123" since BasicInvitationSeeder sets orgId=123 usually.
                TestAuthHandler.TestClaims.Add(new Claim("OrgAdmin", "123"));

                var client = factory.CreateClient();
                var FakeInvitationId = "fakeInvitation";

                // Act
                var response = await client.DeleteAsync($"/invitations/{FakeInvitationId}");

                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
            finally
            {
                TestAuthHandler.TestClaims.Clear();
                TestAuthHandler.TestClaims.AddRange(originalClaims);
            }
        }
        #endregion*/
    }
}