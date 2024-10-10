using System.ComponentModel.DataAnnotations;

namespace GirafAPI.Entities.DTOs;

public record UpdateUsernameDTO(
    [Required] [StringLength(50)] string Username
);