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
using GirafAPI.Entities.Invitations;


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
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            seeder.SeedSingleUser(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            seeder.SeedInvitation(
                scope.ServiceProvider.GetRequiredService<GirafDbContext>(),
                seeder.Organizations[0].Id,
                seeder.Users["owner"].Id,
                seeder.Users["user"].Id
                );
            var client = factory.CreateClient();

            TestAuthHandler.SetTestClaims(scope, seeder.Users["owner"]);

            // Act
            var invitationId = seeder.Invitations.First().Id;
            var response = await client.GetAsync($"/invitations/{invitationId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var invitation = await response.Content.ReadFromJsonAsync<Invitation>();
            Assert.NotNull(invitation);
        }

        //2. Tests if you get a Not Found if invitation doesn't exist
        [Fact] 
        public async Task GetInvitationById_ReturnsNotFound_WhenInvitationDoesnotExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            TestAuthHandler.SetTestClaims(scope, seeder.Users["owner"]);
            
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
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            seeder.SeedSingleUser(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            seeder.SeedInvitation(
                scope.ServiceProvider.GetRequiredService<GirafDbContext>(),
                seeder.Organizations[0].Id,
                "badId",
                seeder.Users["user"].Id
            );
            var client = factory.CreateClient();

            TestAuthHandler.SetTestClaims(scope, seeder.Users["owner"]);
        
            // Act
            var invitationId = seeder.Invitations.First().Id;
            var response = await client.GetAsync($"/invitations/{invitationId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        //4. Tests if you get a Not Found if organization doesn't exist
        [Fact] 
        public async Task GetInvitationById_ReturnsNotFound_WhenOrganizationDoesnotExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            seeder.SeedSingleUser(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            seeder.SeedInvitation(
                scope.ServiceProvider.GetRequiredService<GirafDbContext>(),
                -1,
                seeder.Users["owner"].Id,
                seeder.Users["user"].Id
            );
            var client = factory.CreateClient();

            TestAuthHandler.SetTestClaims(scope, seeder.Users["owner"]);
        
            // Act
            var invitationId = seeder.Invitations.First().Id;
            var response = await client.GetAsync($"/invitations/{invitationId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion
        
        #region Get Invitation by User ID Tests - Test 5-8
        //5. Tests if you can succesfully get an invitation with the recievers id and get no errors
        [Fact] 
        public async Task GetUserInvitation_ReturnsInvitation_WhenInvitationExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            seeder.SeedSingleUser(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            seeder.SeedInvitation(
                scope.ServiceProvider.GetRequiredService<GirafDbContext>(),
                seeder.Organizations[0].Id,
                seeder.Users["owner"].Id,
                seeder.Users["user"].Id
            );
            var client = factory.CreateClient();

            TestAuthHandler.SetTestClaims(scope, seeder.Users["user"]);

            // Act
            var userId = seeder.Users["user"].Id;
            var response = await client.GetAsync($"/invitations/user/{userId}");

            // Assert
            response.EnsureSuccessStatusCode();
        }
        
        //6. Tests if you get a Not Found if user doesn't have an invitation
        [Fact] 
        public async Task GetUserInvitation_ReturnsNotFound_WhenNoInvitationExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            seeder.SeedSingleUser(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            var client = factory.CreateClient();

            TestAuthHandler.SetTestClaims(scope, seeder.Users["user"]);
            
            var fakeId = 123;

            // Act
            var response = await client.GetAsync($"/invitations/user/{fakeId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        //7. Tests if you get a Not Found if invitation is found but sender is null
        [Fact] 
        public async Task GetUserInvitation_ReturnsNotFound_WhenInvitationExistsButSenderIsNull()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            seeder.SeedSingleUser(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            seeder.SeedInvitation(
                scope.ServiceProvider.GetRequiredService<GirafDbContext>(),
                seeder.Organizations[0].Id,
                "badId",
                seeder.Users["user"].Id
            );
            var client = factory.CreateClient();

            TestAuthHandler.SetTestClaims(scope, seeder.Users["user"]);

            // Act
            var userId = seeder.Users["user"].Id;
            var response = await client.GetAsync($"/invitations/user/{userId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        //8. Tests if you get a Not Found if invitation is found but organization is null
        [Fact] 
        public async Task GetUserInvitation_ReturnsNotFound_WhenInvitationExistsButOrganizationIsNull()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            seeder.SeedSingleUser(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            seeder.SeedInvitation(
                scope.ServiceProvider.GetRequiredService<GirafDbContext>(),
                -1,
                seeder.Users["owner"].Id,
                seeder.Users["user"].Id
            );
            var client = factory.CreateClient();

            TestAuthHandler.SetTestClaims(scope, seeder.Users["user"]);

            // Act
            var userId = seeder.Users["user"].Id;
            var response = await client.GetAsync($"/invitations/user/{userId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region Get Invitation by Organization ID - Tests 9-11

        //9. Tests if you can succesfully get an invitation with the organizations id and get no errors
        
        [Fact] 
        public async Task GetOrganizationInvitation_ReturnsInvitation_WhenInvitationExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            seeder.SeedSingleUser(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            seeder.SeedInvitation(
                scope.ServiceProvider.GetRequiredService<GirafDbContext>(),
                seeder.Organizations[0].Id,
                seeder.Users["owner"].Id,
                seeder.Users["user"].Id
            );
            var client = factory.CreateClient();

            TestAuthHandler.SetTestClaims(scope, seeder.Users["owner"]);

            // Act
            var orgId = seeder.Organizations[0].Id;
            var response = await client.GetAsync($"/invitations/org/{orgId}");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        //10. Tests if you get a Not Found if invitation doesn't match organzation Id
        
        [Fact] 
        public async Task GetOrganizationInvitation_ReturnsNotFound_WhenNoInvitationWithValidOrganizationIdExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            seeder.SeedSingleUser(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            seeder.SeedInvitation(
                scope.ServiceProvider.GetRequiredService<GirafDbContext>(),
                -1,
                seeder.Users["owner"].Id,
                seeder.Users["user"].Id
            );
            var client = factory.CreateClient();

            TestAuthHandler.SetTestClaims(scope, seeder.Users["owner"]);

            // Act
            var orgId = seeder.Organizations[0].Id;
            var response = await client.GetAsync($"/invitations/org/{orgId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        //11. Tests if you get a Not Found if invitation doesn't have a valid sender
        
        [Fact] 
        public async Task GetOrganizationInvitation_ReturnsNotFound_WhenNoValidSenderExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            seeder.SeedSingleUser(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            seeder.SeedInvitation(
                scope.ServiceProvider.GetRequiredService<GirafDbContext>(),
                seeder.Organizations[0].Id,
                "badId",
                seeder.Users["user"].Id
            );
            var client = factory.CreateClient();

            TestAuthHandler.SetTestClaims(scope, seeder.Users["owner"]);

            // Act
            var orgId = seeder.Organizations[0].Id;
            var response = await client.GetAsync($"/invitations/org/{orgId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        } 
        
        #endregion
    
        #region Post Invitation - Tests 12-13

        [Fact]
        //12. Succesfully posts a new invitation
        public async Task PostInvitation_ReturnsCreated_IfSucessfullyCreated()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            seeder.SeedSingleUser(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            var client = factory.CreateClient();

            TestAuthHandler.SetTestClaims(scope, seeder.Users["owner"]);

            var sender = seeder.Users["owner"];
            var receiver = seeder.Users["user"];
            var orgId = seeder.Organizations[0].Id;

            var newInvitationDto = new CreateInvitationDTO(orgId, receiver.Email, sender.Id);

            // Act
            var response = await client.PostAsJsonAsync("/invitations/", newInvitationDto);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        //13. Throws bad request if reciever is Bad Request.
        public async Task PostInvitation_ReturnsBadRequest_IfRecieverNotFound()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            seeder.SeedSingleUser(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            var client = factory.CreateClient();

            TestAuthHandler.SetTestClaims(scope, seeder.Users["owner"]);

            var sender = seeder.Users["owner"];
            var fakeRecieverEmail = "fake@email.com";
            var orgId = seeder.Organizations[0].Id;

            var newInvitationDto = new CreateInvitationDTO(orgId, fakeRecieverEmail, sender.Id);

            // Act
            var response = await client.PostAsJsonAsync("/invitations/", newInvitationDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        #endregion
    
        #region Put Invitation by ID and response - Test 14-18
        
        //14. Tests if you can succesfully accept an invitaion
        [Fact]
        public async Task PutInvitationById_ReturnsOk_WhenInvitationIsAccepted()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            seeder.SeedSingleUser(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            seeder.SeedInvitation(
                scope.ServiceProvider.GetRequiredService<GirafDbContext>(),
                seeder.Organizations[0].Id,
                seeder.Users["owner"].Id,
                seeder.Users["user"].Id
            );
            var client = factory.CreateClient();

            TestAuthHandler.SetTestClaims(scope, seeder.Users["user"]);

            var responseDto = new InvitationResponseDTO { Response = true };

            // Act
            var invitationId = seeder.Invitations[0].Id;
            var response = await client.PutAsJsonAsync($"/invitations/respond/{invitationId}", responseDto);

            // Assert
            response.EnsureSuccessStatusCode();
            
            var verificationScope = factory.Services.CreateScope();
            var dbContext = verificationScope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var deletedInvitation = await dbContext.Invitations.FindAsync(invitationId);
            Assert.Null(deletedInvitation);
        }

        //15. Tests if you can succesfully decline an invitaion
        [Fact]
        public async Task PutInvitationById_ReturnsOk_WhenInvitationIsDeclined()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            seeder.SeedSingleUser(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            seeder.SeedInvitation(
                scope.ServiceProvider.GetRequiredService<GirafDbContext>(),
                seeder.Organizations[0].Id,
                seeder.Users["owner"].Id,
                seeder.Users["user"].Id
            );
            var client = factory.CreateClient();

            TestAuthHandler.SetTestClaims(scope, seeder.Users["user"]);

            var responseDto = new InvitationResponseDTO { Response = false };

            // Act
            var invitationId = seeder.Invitations[0].Id;
            var response = await client.PutAsJsonAsync($"/invitations/respond/{invitationId}", responseDto);

            // Assert
            response.EnsureSuccessStatusCode();

            var verificationScope = factory.Services.CreateScope();
            var dbContext = verificationScope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var deletedInvitation = await dbContext.Invitations.FindAsync(invitationId);
            Assert.Null(deletedInvitation);
        }

        //16. Test that makes sure a non-existing invitation Id returns Bad Request.
        [Fact]
        public async Task PutInvitationById_ReturnsNotFound_WhenInvitationIdDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            TestAuthHandler.SetTestClaims(scope, seeder.Users["owner"]);

            var FakeInvitaionId = "fakeInvitation";

            var responseDto = new InvitationResponseDTO { Response = true };

            // Act
            var response = await client.PutAsJsonAsync($"/invitations/respond/{FakeInvitaionId}", responseDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        //17. Test that makes sure that an accepted invitation without an organization returns Not Found
        [Fact]
        public async Task PutInvitationById_ReturnsNotFound_WhenInvitationDoesNotHaveAnOrganization()
        {
             // Arrange
             var factory = new GirafWebApplicationFactory();
             var seeder = new OnlyUsersAndOrgDb();
             var scope = factory.Services.CreateScope();
             factory.SeedDb(scope, seeder);
             seeder.SeedSingleUser(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
             seeder.SeedInvitation(
                 scope.ServiceProvider.GetRequiredService<GirafDbContext>(),
                 -1,
                 seeder.Users["owner"].Id,
                 seeder.Users["user"].Id
             );
             var client = factory.CreateClient();

             TestAuthHandler.SetTestClaims(scope, seeder.Users["owner"]);

            var responseDto = new InvitationResponseDTO { Response = true };

            // Act
            var invitationId = seeder.Invitations[0].Id;
            var response = await client.PutAsJsonAsync($"/invitations/respond/{invitationId}", responseDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        //18. Test that makes sure that an accepted invitation without an reciever returns Not Found
        [Fact]
        public async Task PutInvitationById_ReturnsNotFound_WhenInvitationDoesNotHaveAReciever()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            seeder.SeedSingleUser(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            seeder.SeedInvitation(
                scope.ServiceProvider.GetRequiredService<GirafDbContext>(),
                seeder.Organizations[0].Id,
                seeder.Users["owner"].Id,
                "badId"
            );
            var client = factory.CreateClient();

            TestAuthHandler.SetTestClaims(scope, seeder.Users["owner"]);

            var responseDto = new InvitationResponseDTO { Response = true };

            // Act
            var invitationId = seeder.Invitations[0].Id;
            var response = await client.PutAsJsonAsync($"/invitations/respond/{invitationId}", responseDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion
    
        #region Delete Invitation by ID - Test 18-

        //18. Tests if you can succesfully delete an invitaion
        [Fact]
        public async Task DeleteInvitationById_ReturnsOk_WhenInvitationIsDeleted()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            seeder.SeedSingleUser(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            seeder.SeedInvitation(
                scope.ServiceProvider.GetRequiredService<GirafDbContext>(),
                seeder.Organizations[0].Id,
                seeder.Users["owner"].Id,
                seeder.Users["user"].Id
            );
            var client = factory.CreateClient();

            TestAuthHandler.SetTestClaims(scope, seeder.Users["owner"]);

            // Act
            var invitationId = seeder.Invitations[0].Id;
            var response = await client.DeleteAsync($"/invitations/{invitationId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var verificationScope = factory.Services.CreateScope();
            var dbContext = verificationScope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var deletedInvitation = await dbContext.Invitations.FindAsync(invitationId);
            Assert.Null(deletedInvitation);
        }

        //19. Test that makes sure that endpoint returns Bad Request when invitation does not exist
        [Fact]
        public async Task DeleteInvitationById_ReturnsBadRequest_WhenInvitationIdDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            TestAuthHandler.SetTestClaims(scope, seeder.Users["owner"]);

            var FakeInvitaionId = "fakeInvitation";

            // Act
            var response = await client.DeleteAsync($"/invitations/{FakeInvitaionId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion
    }
}