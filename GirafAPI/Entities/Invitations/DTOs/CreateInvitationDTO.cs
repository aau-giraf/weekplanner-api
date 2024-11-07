using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;

namespace GirafAPI.Entities.Invitations.DTOs;

public record CreateInvitationDTO(
    int OrganizationId,
    string ReceiverEmail,
    string SenderId
    );