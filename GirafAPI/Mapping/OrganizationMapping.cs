using GirafAPI.Entities.DTOs;
using GirafAPI.Entities.Grades;
using GirafAPI.Entities.Grades.DTOs;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Organizations.DTOs;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Citizens.DTOs;
using GirafAPI.Entities.Users;

namespace GirafAPI.Mapping;

public static class OrganizationMapping
{
    public static Organization ToEntity(this CreateOrganizationDTO newOrganization, GirafUser user)
    {
        var organization = new Organization
        {
            Name = newOrganization.Name,
            Users = new List<GirafUser>(),
            Citizens = new List<Citizen>(),
            Grades = new List<Grade>()
        };
        
        organization.Users.Add(user);
        
        return organization;
    }
    
    public static OrganizationDTO ToDTO(this Organization organization)
    {
        var users = new List<UserDTO>();
        foreach (var user in organization.Users)
        {
            users.Add(user.ToDTO());
        }
        
        
        var citizens = new List<CitizenDTO>();
        if (organization.Citizens is not null)
        {
            foreach (var citizen in organization.Citizens)
            {
                citizens.Add(citizen.ToDTO());
            }
        }
        
        var grades = new List<GradeDTO>();
        if (organization.Grades is not null)
        {
            foreach (var grade in organization.Grades)
            {
                grades.Add(grade.ToDTO());
            }
        }
        
        return new OrganizationDTO(
            organization.Id,
            organization.Name,
            users,
            citizens,
            grades
            );
    }

    public static OrganizationNameOnlyDTO ToNameOnlyDTO(this Organization organization)
    {
        return new OrganizationNameOnlyDTO(
            organization.Id,
            organization.Name
            );
    }
}