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
            var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
            var client = factory.CreateClient();

            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var existingInvitation = await dbContext.Invitations.FirstAsync();
            Assert.NotNull(existingInvitation);

            // Act
            var response = await client.GetAsync($"/invitations/{existingInvitation.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.NotNull(existingInvitation);
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
            var client = factory.CreateClient();

            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var existingInvitation = await dbContext.Invitations.FirstAsync();
            Assert.NotNull(existingInvitation);

            existingInvitation.SenderId = "";
            await dbContext.SaveChangesAsync();
        
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
            var client = factory.CreateClient();

            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var existingInvitation = await dbContext.Invitations.FirstAsync();
            Assert.NotNull(existingInvitation);

            //The current test organization has an ID of 123
            existingInvitation.OrganizationId = 321;
            await dbContext.SaveChangesAsync();
        
            // Act
            var response = await client.GetAsync($"/invitations/{existingInvitation.Id}");

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
            var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
            var client = factory.CreateClient();

            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var existingRecievingUser = await dbContext.Users.FirstOrDefaultAsync();
            Assert.NotNull(existingRecievingUser);


            // Act
            var response = await client.GetAsync($"/invitations/user/{existingRecievingUser.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
        }
        
        //6. Tests if you get a Not Found if user doesn't have an invitation
        [Fact] 
        public async Task GetUserInvitation_ReturnsNotFound_WhenNoInvitationExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(sp => new BasicUserSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
            var client = factory.CreateClient();
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
            var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
            var client = factory.CreateClient();
            
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var existingRecievingUser = await dbContext.Users.FirstOrDefaultAsync();
            Assert.NotNull(existingRecievingUser);

            var existingInvitation = await dbContext.Invitations.FirstOrDefaultAsync();
            Assert.NotNull(existingInvitation);
            existingInvitation.SenderId = "";

            // Act
            var response = await client.GetAsync($"/invitations/user/{existingRecievingUser}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        //8. Tests if you get a Not Found if invitation is found but organization is null
        [Fact] 
        public async Task GetUserInvitation_ReturnsNotFound_WhenInvitationExistsButOrganizationIsNull()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
            var client = factory.CreateClient();
            
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var existingRecievingUser = await dbContext.Users.FirstOrDefaultAsync();
            Assert.NotNull(existingRecievingUser);

            var existingInvitation = await dbContext.Invitations.FirstOrDefaultAsync();
            Assert.NotNull(existingInvitation);
            //The current test organization has an ID of 123
            existingInvitation.OrganizationId = 321;

            // Act
            var response = await client.GetAsync($"/invitations/user/{existingRecievingUser}");

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
            var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
            var client = factory.CreateClient();

            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var existingOrganization = await dbContext.Organizations.FirstOrDefaultAsync();
            Assert.NotNull(existingOrganization);

            // Act
            var response = await client.GetAsync($"/invitations/org/{existingOrganization.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        //10. Tests if you get a Not Found if invitation doesn't match organzation Id
        
        [Fact] 
        public async Task GetOrganizationInvitation_ReturnsNotFound_WhenNoInvitationWithValidOrganizationIdExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
            var client = factory.CreateClient();

            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var existingOrganization = await dbContext.Organizations.FirstOrDefaultAsync();
            Assert.NotNull(existingOrganization);

            var existingInvitation = await dbContext.Invitations.FirstOrDefaultAsync();
            Assert.NotNull(existingInvitation);
            //The current test organization has an ID of 123
            existingInvitation.OrganizationId = 321;
            dbContext.SaveChanges();

            // Act
            var response = await client.GetAsync($"/invitations/org/{existingOrganization.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        //11. Tests if you get a Not Found if invitation doesn't have a valid sender
        
        [Fact] 
        public async Task GetOrganizationInvitation_ReturnsNotFound_WhenNoValidSenderExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
            var client = factory.CreateClient();

            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var existingOrganization = await dbContext.Organizations.FirstOrDefaultAsync();
            Assert.NotNull(existingOrganization);

            var existingInvitation = await dbContext.Invitations.FirstOrDefaultAsync();
            Assert.NotNull(existingInvitation);
            existingInvitation.SenderId = "";
            dbContext.SaveChanges();

            // Act
            var response = await client.GetAsync($"/invitations/org/{existingOrganization.Id}");

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
            var factory = new GirafWebApplicationFactory(sp => new UserWithOrganizationsSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
            var client = factory.CreateClient();

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

            if (!createResult.Succeeded)
            {
                throw new Exception($"Failed to create receiver user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }

            var existingOrganization = await dbContext.Organizations.FirstOrDefaultAsync();
            Assert.NotNull(existingOrganization);

            var newInvitationDto = new CreateInvitationDTO(existingOrganization.Id, receiver.Email, sender.Id);

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
            var factory = new GirafWebApplicationFactory(sp => new OrganizationWithUserSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
            var client = factory.CreateClient();

            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>();

            var sender = await dbContext.Users.FirstOrDefaultAsync();
            Assert.NotNull(sender);

            var existingOrganization = await dbContext.Organizations.FirstOrDefaultAsync();
            Assert.NotNull(existingOrganization);

            var fakeRecieverEmail = "fake@email.com";

            var newInvitationDto = new CreateInvitationDTO(existingOrganization.Id, fakeRecieverEmail, sender.Id);

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
            var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
            var client = factory.CreateClient();

            // Retrieve the actual user ID
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var existingInvitation = await dbContext.Invitations.FirstOrDefaultAsync();
            Assert.NotNull(existingInvitation);

            var responseDto = new InvitationResponseDTO { Response = true };

            // Act
            var response = await client.PutAsJsonAsync($"/invitations/respond/{existingInvitation.Id}", responseDto);

            // Assert
            response.EnsureSuccessStatusCode();

            // Detach the existingInvitation entity to ensure a fresh query
            dbContext.Entry(existingInvitation).State = EntityState.Detached;

            // Reload the invitation from the database
            var deletedInvitation = await dbContext.Invitations.FindAsync(existingInvitation.Id);
            Assert.Null(deletedInvitation);
        }

        //15. Tests if you can succesfully decline an invitaion
        [Fact]
        public async Task PutInvitationById_ReturnsOk_WhenInvitationIsDeclined()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
            var client = factory.CreateClient();

            // Retrieve the actual user ID
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var existingInvitation = await dbContext.Invitations.FirstOrDefaultAsync();
            Assert.NotNull(existingInvitation);

            var responseDto = new InvitationResponseDTO { Response = false };

            // Act
            var response = await client.PutAsJsonAsync($"/invitations/respond/{existingInvitation.Id}", responseDto);

            // Assert
            response.EnsureSuccessStatusCode();

            // Detach the existingInvitation entity to ensure a fresh query
            dbContext.Entry(existingInvitation).State = EntityState.Detached;

            // Reload the invitation from the database
            var deletedInvitation = await dbContext.Invitations.FindAsync(existingInvitation.Id);
            Assert.Null(deletedInvitation);
        }

        //16. Test that makes sure a non-existing invitation Id returns Bad Request.
        [Fact]
        public async Task PutInvitationById_ReturnsNotFound_WhenInvitationIdDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
            var client = factory.CreateClient();

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
            var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
            var client = factory.CreateClient();

            // Retrieve the actual user ID
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var existingInvitation = await dbContext.Invitations.FirstOrDefaultAsync();
            Assert.NotNull(existingInvitation);

            //Organization ID in the database is 123
            existingInvitation.OrganizationId = 1000;
            await dbContext.SaveChangesAsync();

            var responseDto = new InvitationResponseDTO { Response = true };

            // Act
            var response = await client.PutAsJsonAsync($"/invitations/respond/{existingInvitation.Id}", responseDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        //18. Test that makes sure that an accepted invitation without an reciever returns Not Found
        [Fact]
        public async Task PutInvitationById_ReturnsNotFound_WhenInvitationDoesNotHaveAReciever()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
            var client = factory.CreateClient();

            // Retrieve the actual user ID
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var existingInvitation = await dbContext.Invitations.FirstOrDefaultAsync();
            Assert.NotNull(existingInvitation);

            existingInvitation.ReceiverId = "";
            await dbContext.SaveChangesAsync();

            var responseDto = new InvitationResponseDTO { Response = true };

            // Act
            var response = await client.PutAsJsonAsync($"/invitations/respond/{existingInvitation.Id}", responseDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion
    
        #region Delete Invitation by ID - Test 18-

        //18. Tests if you can succesfully accept an invitaion
        [Fact]
        public async Task DeleteInvitationById_ReturnsOk_WhenInvitationIsDeleted()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
            var client = factory.CreateClient();

            // Retrieve the actual user ID
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var existingInvitation = await dbContext.Invitations.FirstOrDefaultAsync();
            Assert.NotNull(existingInvitation);

            // Act
            var response = await client.DeleteAsync($"/invitations/{existingInvitation.Id}");

            // Assert
            response.EnsureSuccessStatusCode();

            // Detach the existingInvitation entity to ensure a fresh query
            dbContext.Entry(existingInvitation).State = EntityState.Detached;

            // Reload the invitation from the database
            var deletedInvitation = await dbContext.Invitations.FindAsync(existingInvitation.Id);
            Assert.Null(deletedInvitation);
        }

        //19. Test that makes sure that endpoint returns Bad Request when invitation does not exist
        [Fact]
        public async Task DeleteInvitationById_ReturnsBadRequest_WhenInvitationIdDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(sp => new BasicInvitationSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
            var client = factory.CreateClient();

            var FakeInvitaionId = "fakeInvitation";

            // Act
            var response = await client.DeleteAsync($"/invitations/{FakeInvitaionId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion
    }
}