using System.Net;
using System.Net.Http.Json;
using Giraf.IntegrationTests.Utils;
using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Data;
using GirafAPI.Entities.Activities.DTOs;
using GirafAPI.Entities.Pictograms;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Giraf.IntegrationTests.Endpoints
{
    public class ActivityEndpointTests
    {
        #region GET /weekplan/ - Get all activities

        [Fact]
        public async Task GetAllActivities_ReturnsListOfActivities_WhenActivitiesExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(_ => new BasicActivitySeeder());
            var client = factory.CreateClient();

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
            var factory = new GirafWebApplicationFactory(sp => new BasicUserSeeder(sp.GetRequiredService<UserManager<GirafUser>>()));
            var client = factory.CreateClient();

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
            var factory = new GirafWebApplicationFactory(_ => new CitizenWithActivitiesSeeder());
            var client = factory.CreateClient();
            var date = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");

            int citizenId;
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var citizen = await dbContext.Citizens.FirstOrDefaultAsync();
                Assert.NotNull(citizen);
                citizenId = citizen.Id;
            }

            // Act
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
            var factory = new GirafWebApplicationFactory(_ => new EmptyDb());
            var client = factory.CreateClient();
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
            var factory = new GirafWebApplicationFactory(_ => new GradeWithActivitiesSeeder());
            var client = factory.CreateClient();
            var date = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");

            int gradeId;
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var grade = await dbContext.Grades.FirstOrDefaultAsync();
                Assert.NotNull(grade);
                gradeId = grade.Id;
            }

            // Act
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
            var factory = new GirafWebApplicationFactory(_ => new EmptyDb());
            var client = factory.CreateClient();
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
            var factory = new GirafWebApplicationFactory(_ => new BasicActivitySeeder());
            var client = factory.CreateClient();

            int activityId;
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var activity = await dbContext.Activities.FirstOrDefaultAsync();
                Assert.NotNull(activity);
                activityId = activity.Id;
            }

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
            var factory = new GirafWebApplicationFactory(_ => new EmptyDb());
            var client = factory.CreateClient();
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
            var factory = new GirafWebApplicationFactory(_ => new BasicCitizenSeeder());
            var client = factory.CreateClient();

            int citizenId;
            int pictogramId;

            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var citizen = await dbContext.Citizens.Include(c => c.Organization).FirstOrDefaultAsync();
                Assert.NotNull(citizen);
                citizenId = citizen.Id;

                // Create a pictogram associated with the citizen's organization
                var pictogram = new Pictogram
                {
                    PictogramName = "Test Pictogram",
                    PictogramUrl = "http://example.com/pictogram.png",
                    OrganizationId = citizen.Organization.Id
                };
                dbContext.Pictograms.Add(pictogram);
                await dbContext.SaveChangesAsync();
                pictogramId = pictogram.Id;
            }

            var newActivityDto = new CreateActivityDTO
            (
                Date: DateOnly.FromDateTime(DateTime.UtcNow),
                Name: "Test Activity",
                Description: "This is a test activity",
                StartTime: TimeOnly.FromDateTime(DateTime.UtcNow),
                EndTime: TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)),
                PictogramId: pictogramId // Provide the valid PictogramId
            );

            // Act
            var response = await client.PostAsJsonAsync($"/weekplan/to-citizen/{citizenId}", newActivityDto);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var activityDto = await response.Content.ReadFromJsonAsync<ActivityDTO>();
            Assert.NotNull(activityDto);
            Assert.Equal(newActivityDto.Name, activityDto.Name);
        }


        [Fact]
        public async Task CreateActivityForCitizen_ReturnsNotFound_WhenCitizenDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(_ => new EmptyDb());
            var client = factory.CreateClient();
            var nonExistentCitizenId = 999;

            var newActivityDto = new CreateActivityDTO
            (
                Date: DateOnly.FromDateTime(DateTime.UtcNow),
                Name: "Test Activity",
                Description: "This is a test activity",
                StartTime: TimeOnly.FromDateTime(DateTime.UtcNow),
                EndTime: TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)),
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
            var factory = new GirafWebApplicationFactory(_ => new GradeWithActivitiesSeeder());
            var client = factory.CreateClient();

            int gradeId;
            int pictogramId;

            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();

                // Retrieve the grade
                var grade = await dbContext.Grades.FirstOrDefaultAsync();
                Assert.NotNull(grade);
                gradeId = grade.Id;

                // Retrieve the organization ID associated with the grade
                int organizationId = grade.OrganizationId;

                // Create a pictogram associated with the grade's organization
                var pictogram = new Pictogram
                {
                    PictogramName = "Test Pictogram",
                    PictogramUrl = "http://example.com/pictogram.png",
                    OrganizationId = organizationId
                };
                dbContext.Pictograms.Add(pictogram);
                await dbContext.SaveChangesAsync();
                pictogramId = pictogram.Id;
            }

            var newActivityDto = new CreateActivityDTO
            (
                Date: DateOnly.FromDateTime(DateTime.UtcNow),
                Name: "Test Activity",
                Description: "This is a test activity",
                StartTime: TimeOnly.FromDateTime(DateTime.UtcNow),
                EndTime: TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)),
                PictogramId: pictogramId // Provide the valid PictogramId
            );

            // Act
            var response = await client.PostAsJsonAsync($"/weekplan/to-grade/{gradeId}", newActivityDto);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var activityDto = await response.Content.ReadFromJsonAsync<ActivityDTO>();
            Assert.NotNull(activityDto);
            Assert.Equal(newActivityDto.Name, activityDto.Name);
        }


        [Fact]
        public async Task CreateActivityForGrade_ReturnsNotFound_WhenGradeDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(_ => new EmptyDb());
            var client = factory.CreateClient();
            var nonExistentGradeId = 999;

            var newActivityDto = new CreateActivityDTO
            (
                Date: DateOnly.FromDateTime(DateTime.UtcNow),
                Name: "Test Activity",
                Description: "This is a test activity",
                StartTime: TimeOnly.FromDateTime(DateTime.UtcNow),
                EndTime: TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)),
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
            var factory = new GirafWebApplicationFactory(_ => new BasicActivitySeeder());
            var client = factory.CreateClient();

            int activityId;
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var activity = await dbContext.Activities.FirstOrDefaultAsync();
                Assert.NotNull(activity);
                activityId = activity.Id;
            }

            var updateActivityDto = new UpdateActivityDTO
            (
                Name: "Updated Activity",
                Description: "Updated description",
                Date: DateOnly.FromDateTime(DateTime.UtcNow),
                StartTime: TimeOnly.FromDateTime(DateTime.UtcNow),
                EndTime: TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)),
                IsCompleted: true,
                PictogramId: 10000,
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
                Assert.Equal("Updated Activity", updatedActivity.Name);
                Assert.Equal("Updated description", updatedActivity.Description);
                Assert.Equal(updateActivityDto.IsCompleted, updatedActivity.IsCompleted);
            }
        }

        [Fact]
        public async Task UpdateActivity_ReturnsNotFound_WhenActivityDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(_ => new EmptyDb());
            var client = factory.CreateClient();
            var nonExistentActivityId = 999;

            var newActivityDto = new CreateActivityDTO
            (
                Date: DateOnly.FromDateTime(DateTime.UtcNow),
                Name: "Test Activity",
                Description: "This is a test activity",
                StartTime: TimeOnly.FromDateTime(DateTime.UtcNow),
                EndTime: TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)),
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
            var factory = new GirafWebApplicationFactory(_ => new BasicActivitySeeder());
            var client = factory.CreateClient();

            int activityId;
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var activity = await dbContext.Activities.FirstOrDefaultAsync();
                Assert.NotNull(activity);
                activityId = activity.Id;
            }

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
            var factory = new GirafWebApplicationFactory(_ => new EmptyDb());
            var client = factory.CreateClient();
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
            var factory = new GirafWebApplicationFactory(_ => new BasicActivitySeeder());
            var client = factory.CreateClient();

            int activityId;
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var activity = await dbContext.Activities.FirstOrDefaultAsync();
                Assert.NotNull(activity);
                activityId = activity.Id;
            }

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
            var factory = new GirafWebApplicationFactory(_ => new EmptyDb());
            var client = factory.CreateClient();
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
            var factory = new GirafWebApplicationFactory(_ => new ActivityAndPictogramSeeder());
            var client = factory.CreateClient();

            int activityId;
            int pictogramId;
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var activity = await dbContext.Activities.FirstOrDefaultAsync();
                var pictogram = await dbContext.Pictograms.FirstOrDefaultAsync();
                Assert.NotNull(activity);
                Assert.NotNull(pictogram);
                activityId = activity.Id;
                pictogramId = pictogram.Id;
            }

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
            var factory = new GirafWebApplicationFactory(_ => new BasicPictogramSeeder());
            var client = factory.CreateClient();
            var nonExistentActivityId = 999;
            int pictogramId;
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var pictogram = await dbContext.Pictograms.FirstOrDefaultAsync();
                Assert.NotNull(pictogram);
                pictogramId = pictogram.Id;
            }

            // Act
            var response = await client.PostAsync($"/weekplan/activity/assign-pictogram/{nonExistentActivityId}/{pictogramId}", null);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AssignPictogram_ReturnsNotFound_WhenPictogramDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(_ => new BasicActivitySeeder());
            var client = factory.CreateClient();
            var nonExistentPictogramId = 999;
            int activityId;
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var activity = await dbContext.Activities.FirstOrDefaultAsync();
                Assert.NotNull(activity);
                activityId = activity.Id;
            }

            // Act
            var response = await client.PostAsync($"/weekplan/activity/assign-pictogram/{activityId}/{nonExistentPictogramId}", null);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion
    }
}
