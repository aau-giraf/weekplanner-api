using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Organizations.DTOs;

namespace GirafAPI.Mapping
{
    public static class OrganizationMappingExtensions
    {
        public static OrganizationDTO ToDTO(this Organization organization)
        {
            return new OrganizationDTO
            {
                Id = organization.Id,
                Name = organization.Name
            };
        }

        public static Organization ToEntity(this CreateOrganizationDTO dto)
        {
            return new Organization
            {
                Name = dto.Name
            };
        }

        public static void UpdateFromDTO(this Organization organization, UpdateOrganizationDTO dto)
        {
            organization.Name = dto.Name;
        }
    }
}