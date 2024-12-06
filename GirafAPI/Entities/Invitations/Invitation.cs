using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;

namespace GirafAPI.Entities.Invitations;

public class Invitation
{
    public int Id { get; set; }

    [Required]
    public string ReceiverId { get; set; }

    [ForeignKey(nameof(ReceiverId))]
    public virtual GirafUser Receiver { get; set; }

    [Required]
    public string SenderId { get; set; }

    [ForeignKey(nameof(SenderId))]
    public virtual GirafUser Sender { get; set; }

    [Required]
    public int OrganizationId { get; set; }

    [ForeignKey("OrganizationId")]
    public virtual Organization Organization { get; set; }
}