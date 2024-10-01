﻿using System.ComponentModel.DataAnnotations;

namespace GirafAPI.Entities.Resources.DTOs;

// Data necessary to create a Citizen
public record CreateCitizenDTO(
    [Required][StringLength(50)] string FirstName,
    [Required][StringLength(20)] string LastName,
    [Required][StringLength(50)] string Username,
    [Required][StringLength(100)] string Password
);