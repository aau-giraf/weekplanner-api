using GirafAPI.Data;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Organizations.DTOs;
using GirafAPI.Entities.Users;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks; // For asynchronous operations

namespace GirafAPI.Mapping
{
    public static class OrganizationMapping
    {
        // Mapping CreateOrganizationDTO to Organization entity
        public static Organization ToEntity(this CreateOrganizationDTO organizationDTO)
        {
            return new Organization
            {
                Name = organizationDTO.Name
            };
        }

        // Mapping Organization entity to OrganizationDTO
        public static OrganizationDTO ToDTO(this Organization organization)
        {
            return new OrganizationDTO
            {
                Id = organization.Id,
                Name = organization.Name,
                UserIds = organization.Users?.Select(u => u.Id).ToList()
            };
        }

        // Extension method to update the Organization from UpdateOrganizationDTO
        public static void UpdateFromDTO(this Organization organization, UpdateOrganizationDTO dto, GirafDbContext dbContext)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentNullException.ThrowIfNull(dbContext);

            organization.Name = dto.Name;

            if (dto.UserIds != null)
            {
                // Fetch users from the database based on the provided UserIds
                organization.Users = dbContext.Users
                    .Where(u => dto.UserIds.Contains(u.Id))
                    .ToList();
            }
        }
    }
}
