using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;

namespace GirafAPI.Entities.Invitations;

public class Invitation
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public required string ReceiverId { get; set; }
    public required string SenderId { get; set; }
}