using System.Net.Http.Json;
using FluentAssertions;
using Giraf.IntegrationTests.Utils;
using GirafAPI.Entities.Resources.DTOs;

namespace Giraf.IntegrationTests.Endpoints;

public class CitizensEndpointTests
{
    [Fact]
    public async Task PostCitizenWithValidParameters()
    {
        // Arrange
        var application = new GirafWebApplicationFactory();
        CreateCitizenDTO citizen = new CreateCitizenDTO("Hans", "Hansen");
        
        var client = application.CreateClient();
        
        // Act
        var response = await client.PostAsJsonAsync("/citizens", citizen);
        
        // Assert
        response.EnsureSuccessStatusCode();
        
        var createdCitizen = await response.Content.ReadFromJsonAsync<CitizenDTO>();
        createdCitizen?.FirstName.Should().Be("Hans");
        createdCitizen?.LastName.Should().Be("Hansen");
    }
}