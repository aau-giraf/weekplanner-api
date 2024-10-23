using GirafAPI.Entities.Users;
namespace GirafAPI.Entities.Organizations

{
    public class Organization
    {
        public int Id { get; set; }
        public string Name { get; set; }
        // Navigation property
        public ICollection<GirafUser> Users { get; set; }
    }
}