using System.ComponentModel.DataAnnotations;
using GirafAPI.Entities.Activities;

namespace GirafAPI.Entities.Citizens.DTOs;

// Data necessary to view a Citizen
public record CitizenDTO(
    int Id,
    [Required][StringLength(50)] string FirstName,
    [Required][StringLength(20)] string LastName
        );