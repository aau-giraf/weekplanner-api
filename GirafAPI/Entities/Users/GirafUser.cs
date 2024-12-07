using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GirafAPI.Entities.Invitations;
using GirafAPI.Entities.Organizations;
using Microsoft.AspNetCore.Identity;

namespace GirafAPI.Entities.Users;

// Base class for users that stores data common between user types.
public class GirafUser : IdentityUser
{
    [StringLength(20)]
    public override string UserName 
    { 
        get => base.UserName; 
        set => base.UserName = value; 
    }

    [StringLength(20)]
    public override string Email 
    { 
        get => base.Email; 
        set => base.Email = value; 
    }

    // Include NormalizedEmail and NormalizedUserName, for userManager
    public override string NormalizedUserName 
    { 
        get => base.NormalizedUserName; 
        set => base.NormalizedUserName = value; 
    }

    public override string NormalizedEmail 
    { 
        get => base.NormalizedEmail; 
        set => base.NormalizedEmail = value; 
    }

    [StringLength(20)]
    [Required]
    public string FirstName { get; set; }

    [StringLength(50)]
    [Required]
    public string LastName { get; set; }

    public ICollection<Organization> Organizations { get; set; } = new List<Organization>();
    [InverseProperty(nameof(Invitation.Sender))]
    public virtual ICollection<Invitation> SentInvitations { get; set; } = new List<Invitation>();

    [InverseProperty(nameof(Invitation.Receiver))]
    public virtual ICollection<Invitation> ReceivedInvitations { get; set; } = new List<Invitation>();
}