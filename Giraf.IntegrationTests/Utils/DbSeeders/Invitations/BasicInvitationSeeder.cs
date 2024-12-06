using GirafAPI.Data;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Grades;
using GirafAPI.Entities.Invitations;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace Giraf.IntegrationTests.Utils.DbSeeders;
public class BasicInvitationSeeder :  DbSeeder
{
    private readonly UserManager<GirafUser> _userManager;

    public BasicInvitationSeeder(UserManager<GirafUser> userManager)
    {
        _userManager = userManager;
    }
    public override void SeedData(DbContext context)
    {
        var dbContext = (GirafDbContext)context;

        var reciever = new GirafUser
        {
            UserName = "RecieverUser",
            Email = "RecieverUser@example.com",
            FirstName = "RecieverUser",
            LastName = "User",
            Organizations = new List<Organization>()
        };
        var recieverResult = _userManager.CreateAsync(reciever, "Password123!").GetAwaiter().GetResult();

        if (!recieverResult.Succeeded)
        {
            throw new Exception("Failed to create user: " + string.Join(", ", recieverResult.Errors.Select(e => e.Description)));
        }

        var sender = new GirafUser
        {
            UserName = "SenderUser",
            Email = "SenderUser@example.com",
            FirstName = "Sender",
            LastName = "User",
            Organizations = new List<Organization>()
        };
        var senderResult = _userManager.CreateAsync(sender, "Password123!").GetAwaiter().GetResult();

        if (!senderResult.Succeeded)
        {
            throw new Exception("Failed to create user: " + string.Join(", ", senderResult.Errors.Select(e => e.Description)));
        }

        var organization = new Organization
        {
            Id = 123,
            Name = "Basic Test Invitation Organization",
            Users = new List<GirafUser> {sender},
            Citizens = new List<Citizen>(),
            Grades = new List<Grade>()
        };
        dbContext.Organizations.Add(organization);
        dbContext.SaveChanges();

        var testInvitation = new Invitation 
        {  
            OrganizationId = organization.Id,
            ReceiverId = reciever.Id,
            SenderId = sender.Id,
        };
        dbContext.Invitations.Add(testInvitation);
        dbContext.SaveChanges();
        

        Console.WriteLine("[SEEDER] Created RecieverUser: " + reciever.Id);
        Console.WriteLine("[SEEDER] Created SenderUser: " + sender.Id);
        Console.WriteLine("[SEEDER] Created Organization with Id 123 and Invitation with Id=" + testInvitation.Id);
    }   
}

