using System.Net;
using System.Net.Http.Json;
using Giraf.IntegrationTests.Utils;
using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Data;
using GirafAPI.Entities.Activities.DTOs;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Giraf.IntegrationTests.Endpoints
{
    [Collection("IntegrationTests")]
    public class ActivityEndpointTests
    {
        #region GET /weekplan/ - Get all activities

        [Fact]
        public async Task GetAllActivities_ReturnsListOfActivities_WhenActivitiesExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            // Act
            var response = await client.GetAsync("/weekplan");

            // Assert
            response.EnsureSuccessStatusCode();
            var activities = await response.Content.ReadFromJsonAsync<List<ActivityDTO>>();
            Assert.NotNull(activities);
            Assert.NotEmpty(activities);
        }

        [Fact]
        public async Task GetAllActivities_ReturnsEmptyList_WhenNoActivitiesExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["admin"]);

            // Act
            var response = await client.GetAsync("/weekplan");

            // Assert
            response.EnsureSuccessStatusCode();
            var activities = await response.Content.ReadFromJsonAsync<List<ActivityDTO>>();
            Assert.NotNull(activities);
            Assert.Empty(activities);
        }

        #endregion

        #region GET /weekplan/{citizenId:int} - Get activities for a citizen on a date

        [Fact]
        public async Task GetActivitiesForCitizenOnDate_ReturnsActivities_WhenActivitiesExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["admin"]);

            // Act
            int citizenId = seeder.Citizens[0].Id;
            var date = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            var response = await client.GetAsync($"/weekplan/{citizenId}?date={date}");

            // Assert
            response.EnsureSuccessStatusCode();
            var activities = await response.Content.ReadFromJsonAsync<List<ActivityDTO>>();
            Assert.NotNull(activities);
            Assert.NotEmpty(activities);
        }

        [Fact]
        public async Task GetActivitiesForCitizenOnDate_ReturnsNotFound_WhenCitizenDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["admin"]);
            
            var date = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            var nonExistentCitizenId = 999;

            // Act
            var response = await client.GetAsync($"/weekplan/{nonExistentCitizenId}?date={date}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region GET /weekplan/grade/{gradeId:int} - Get activities for a grade on a date

        [Fact]
        public async Task GetActivitiesForGradeOnDate_ReturnsActivities_WhenActivitiesExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);
            
            // Act
            var date = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            int gradeId = seeder.Grades[0].Id;
            var response = await client.GetAsync($"/weekplan/grade/{gradeId}?date={date}");

            // Assert
            response.EnsureSuccessStatusCode();
            var activities = await response.Content.ReadFromJsonAsync<List<ActivityDTO>>();
            Assert.NotNull(activities);
            Assert.NotEmpty(activities);
        }

        [Fact]
        public async Task GetActivitiesForGradeOnDate_ReturnsNotFound_WhenGradeDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);
            
            var date = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            var nonExistentGradeId = 999;

            // Act
            var response = await client.GetAsync($"/weekplan/grade/{nonExistentGradeId}?date={date}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region GET /weekplan/activity/{id:int} - Get activity by ID

        [Fact]
        public async Task GetActivityById_ReturnsActivity_WhenActivityExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            int activityId = seeder.Activities[0].Id;

            // Act
            var response = await client.GetAsync($"/weekplan/activity/{activityId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var activityDto = await response.Content.ReadFromJsonAsync<ActivityDTO>();
            Assert.NotNull(activityDto);
            Assert.Equal(activityId, activityDto.ActivityId);
        }

        [Fact]
        public async Task GetActivityById_ReturnsNotFound_WhenActivityDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);
            
            var nonExistentActivityId = 999;

            // Act
            var response = await client.GetAsync($"/weekplan/activity/{nonExistentActivityId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region POST /weekplan/to-citizen/{citizenId:int} - Create activity for citizen

        [Fact]
        public async Task CreateActivityForCitizen_ReturnsCreated_WhenCitizenExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            int citizenId = seeder.Citizens[0].Id;
            int pictogramId = seeder.Pictograms[0].Id;

            var newActivityDto = new CreateActivityDTO
            (
                Date: DateOnly.FromDateTime(DateTime.UtcNow).ToString(),
                StartTime: TimeOnly.FromDateTime(DateTime.UtcNow).ToString(),
                EndTime: TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)).ToString(),
                PictogramId: pictogramId // Provide the valid PictogramId
            );

            // Act
            var response = await client.PostAsJsonAsync($"/weekplan/to-citizen/{citizenId}", newActivityDto);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var activityDto = await response.Content.ReadFromJsonAsync<ActivityDTO>();
            Assert.NotNull(activityDto);
        }


        [Fact]
        public async Task CreateActivityForCitizen_ReturnsNotFound_WhenCitizenDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);
            
            
            var nonExistentCitizenId = 999;

            var newActivityDto = new CreateActivityDTO
            (
                Date: DateOnly.FromDateTime(DateTime.UtcNow).ToString(),
                StartTime: TimeOnly.FromDateTime(DateTime.UtcNow).ToString(),
                EndTime: TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)).ToString(),
                PictogramId: null
            );

            // Act
            var response = await client.PostAsJsonAsync($"/weekplan/to-citizen/{nonExistentCitizenId}", newActivityDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region POST /weekplan/to-grade/{gradeId:int} - Create activity for grade

        [Fact]
        public async Task CreateActivityForGrade_ReturnsCreated_WhenGradeExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["admin"]);

            int gradeId = seeder.Grades.First().Id;
            int pictogramId = seeder.Pictograms.First().Id;

            var newActivityDto = new CreateActivityDTO
            (
                Date: DateOnly.FromDateTime(DateTime.UtcNow).ToString(),
                StartTime: TimeOnly.FromDateTime(DateTime.UtcNow).ToString(),
                EndTime: TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)).ToString(),
                PictogramId: pictogramId // Provide the valid PictogramId
            );

            // Act
            var response = await client.PostAsJsonAsync($"/weekplan/to-grade/{gradeId}", newActivityDto);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var activityDto = await response.Content.ReadFromJsonAsync<ActivityDTO>();
            Assert.NotNull(activityDto);
        }


        [Fact]
        public async Task CreateActivityForGrade_ReturnsNotFound_WhenGradeDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);
            
            var nonExistentGradeId = 999;

            var newActivityDto = new CreateActivityDTO
            (
                Date: DateOnly.FromDateTime(DateTime.UtcNow).ToString(),
                StartTime: TimeOnly.FromDateTime(DateTime.UtcNow).ToString(),
                EndTime: TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)).ToString(),
                PictogramId: null
            );

            // Act
            var response = await client.PostAsJsonAsync($"/weekplan/to-grade/{nonExistentGradeId}", newActivityDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region PUT /weekplan/activity/{id:int} - Update activity

        [Fact]
        public async Task UpdateActivity_ReturnsOk_WhenActivityExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            int activityId = seeder.Activities[0].Id;
            
            var updateActivityDto = new UpdateActivityDTO
            (
                Date: DateOnly.FromDateTime(DateTime.UtcNow).ToString(),
                StartTime: TimeOnly.FromDateTime(DateTime.UtcNow).ToString(),
                EndTime: TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)).ToString(),
                IsCompleted: true,
                PictogramId: 1,
                CitizenId: 1
            );

            // Act
            var response = await client.PutAsJsonAsync($"/weekplan/activity/{activityId}", updateActivityDto);

            // Assert
            response.EnsureSuccessStatusCode();

            // Verify the activity was updated
            using (var verificationScope = factory.Services.CreateScope())
            {
                var dbContext = verificationScope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var updatedActivity = await dbContext.Activities.FindAsync(activityId);
                Assert.NotNull(updatedActivity);
                Assert.Equal(updateActivityDto.IsCompleted, updatedActivity.IsCompleted);
            }
        }

        [Fact]
        public async Task UpdateActivity_ReturnsNotFound_WhenActivityDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);
            
            var nonExistentActivityId = 999;

            var newActivityDto = new CreateActivityDTO
            (
                Date: DateOnly.FromDateTime(DateTime.UtcNow).ToString(),
                StartTime: TimeOnly.FromDateTime(DateTime.UtcNow).ToString(),
                EndTime: TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)).ToString(),
                PictogramId: null
            );

            // Act
            var response = await client.PutAsJsonAsync($"/weekplan/activity/{nonExistentActivityId}", newActivityDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region DELETE /weekplan/activity/{id:int} - Delete activity

        [Fact]
        public async Task DeleteActivity_ReturnsNoContent_WhenActivityExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            int activityId = seeder.Activities[0].Id;

            // Act
            var response = await client.DeleteAsync($"/weekplan/activity/{activityId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the activity was deleted
            using (var verificationScope = factory.Services.CreateScope())
            {
                var dbContext = verificationScope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var deletedActivity = await dbContext.Activities.FindAsync(activityId);
                Assert.Null(deletedActivity);
            }
        }

        [Fact]
        public async Task DeleteActivity_ReturnsNotFound_WhenActivityDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);
            
            var nonExistentActivityId = 999;

            // Act
            var response = await client.DeleteAsync($"/weekplan/activity/{nonExistentActivityId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region PUT /weekplan/activity/{id:int}/iscomplete - Set activity completion status

        [Fact]
        public async Task SetActivityCompletionStatus_ReturnsOk_WhenActivityExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["admin"]);

            int activityId = seeder.Activities[0].Id;

            var isComplete = true;

            // Act
            var response = await client.PutAsync($"/weekplan/activity/{activityId}/iscomplete?IsComplete={isComplete}", null);

            // Assert
            response.EnsureSuccessStatusCode();

            // Verify the activity's completion status was updated
            using (var verificationScope = factory.Services.CreateScope())
            {
                var dbContext = verificationScope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var updatedActivity = await dbContext.Activities.FindAsync(activityId);
                Assert.NotNull(updatedActivity);
                Assert.Equal(isComplete, updatedActivity.IsCompleted);
            }
        }

        [Fact]
        public async Task SetActivityCompletionStatus_ReturnsNotFound_WhenActivityDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["admin"]);
            
            var nonExistentActivityId = 999;
            var isComplete = true;

            // Act
            var response = await client.PutAsync($"/weekplan/activity/{nonExistentActivityId}/iscomplete?IsComplete={isComplete}", null);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region POST /weekplan/activity/assign-pictogram/{activityId:int}/{pictogramId:int} - Assign pictogram

        [Fact]
        public async Task AssignPictogram_ReturnsOk_WhenActivityAndPictogramExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["admin"]);

            int activityId = seeder.Activities[0].Id;
            int pictogramId = seeder.Pictograms[0].Id;

            // Act
            var response = await client.PostAsync($"/weekplan/activity/assign-pictogram/{activityId}/{pictogramId}", null);

            // Assert
            response.EnsureSuccessStatusCode();
            var activityDto = await response.Content.ReadFromJsonAsync<ActivityDTO>();
            Assert.NotNull(activityDto);
            Assert.Equal(pictogramId, activityId);
        }

        [Fact]
        public async Task AssignPictogram_ReturnsNotFound_WhenActivityDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            seeder.SeedUsers(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            seeder.SeedOrganization(
                scope.ServiceProvider.GetRequiredService<GirafDbContext>(),
                scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>(),
                seeder.Users["owner"],
                new List<GirafUser>(),
                new List<GirafUser>()
                );
            seeder.SeedPictogram(scope.ServiceProvider.GetRequiredService<GirafDbContext>(), seeder.Organizations[0]);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["admin"]);
            
            var nonExistentActivityId = 999;
            int pictogramId = seeder.Pictograms[0].Id;

            // Act
            var response = await client.PostAsync($"/weekplan/activity/assign-pictogram/{nonExistentActivityId}/{pictogramId}", null);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AssignPictogram_ReturnsNotFound_WhenPictogramDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            seeder.SeedUsers(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            seeder.SeedOrganization(
                scope.ServiceProvider.GetRequiredService<GirafDbContext>(),
                scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>(),
                seeder.Users["owner"],
                new List<GirafUser>(),
                new List<GirafUser>()
            );
            seeder.SeedCitizens(scope.ServiceProvider.GetRequiredService<GirafDbContext>(), seeder.Organizations[0]);
            seeder.SeedPictogram(scope.ServiceProvider.GetRequiredService<GirafDbContext>(), seeder.Organizations[0]);
            seeder.SeedCitizenActivity(
                scope.ServiceProvider.GetRequiredService<GirafDbContext>(),
                seeder.Citizens[0].Id,
                seeder.Pictograms[0]);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);
            
            var nonExistentPictogramId = 999;
            int activityId = seeder.Activities[0].Id;

            // Act
            var response = await client.PostAsync($"/weekplan/activity/assign-pictogram/{activityId}/{nonExistentPictogramId}", null);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion
    }
}
