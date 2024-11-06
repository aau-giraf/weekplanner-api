using GirafAPI.Entities.Invitations;
using GirafAPI.Entities.Invitations.DTOs;

namespace GirafAPI.Mapping;

public static class InvitationMapping
{
    public static Invitation ToEntity(this CreateInvitationDTO newInvitation, string receiverId)
    {
        return new Invitation
        {
            OrganizationId = newInvitation.OrganizationId,
            ReceiverId = receiverId,
            SenderId = newInvitation.SenderId
        };
    }

    public static InvitationDTO ToDTO(this Invitation invitation, string organizationName, string senderName)
    {
        return new InvitationDTO(
            invitation.Id,
            invitation.OrganizationId,
            organizationName,
            senderName
            );
    }
}