using System.ComponentModel.DataAnnotations;

namespace GirafAPI.Entities.DTOs;

public record UpdateUserPasswordDTO(
    [Required][StringLength(100)] string oldPassword,
    [Required][StringLength(100)] string newPassword
    );