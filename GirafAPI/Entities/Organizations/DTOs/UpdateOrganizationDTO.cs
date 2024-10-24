namespace GirafAPI.Entities.Organizations.DTOs

{
    public class UpdateOrganizationDTO
    {
        public string Name { get; set; }
        public ICollection<string> UserIds { get; set; }
    }

}