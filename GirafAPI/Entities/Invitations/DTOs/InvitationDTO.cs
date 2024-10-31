using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;

namespace GirafAPI.Entities.Invitations.DTOs;

public record InvitationDTO(
    int Id,
    int OrganizationId,
    string ReceiverId,
    string SenderId
    );