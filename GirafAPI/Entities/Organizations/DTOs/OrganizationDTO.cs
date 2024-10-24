using GirafAPI.Entities.Users;

namespace GirafAPI.Entities.Organizations.DTOs

{
    public class OrganizationDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<string> UserIds { get; set; }
    }
}