﻿using System.ComponentModel.DataAnnotations.Schema;
using GirafAPI.Entities.Weekplans;
using GirafAPI.Entities.Users;

namespace GirafAPI.Entities.Resources;

// Data model of Citizens in the database
public class Citizen
{
    public int Id { get; set; }
    
    public required string FirstName { get; set; }
    
    public required string LastName { get; set; }
    
    public ICollection<Activity>? Activities { get; set; }
}