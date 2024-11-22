namespace GirafAPI.Entities.Invitations.DTOs;

public record CreateInvitationDTO
(
    int OrganizationId,
    string ReceiverEmail,
    string SenderId
);