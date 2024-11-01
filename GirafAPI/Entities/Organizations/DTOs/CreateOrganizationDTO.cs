namespace GirafAPI.Entities.Organizations.DTOs;

public record CreateOrganizationDTO
{
    public required string Name { get; set; }
}