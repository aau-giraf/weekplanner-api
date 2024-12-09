using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GirafAPI.Data;
using GirafAPI.Entities.Activities;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Grades;
using GirafAPI.Entities.Invitations;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Pictograms;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

public abstract class DbSeeder
{
    public Dictionary<String, GirafUser> Users { get; } = new();
    public List<Organization> Organizations { get; } = new();
    public List<Citizen> Citizens { get; } = new();
    public List<Pictogram> Pictograms { get; } = new();
    public List<Activity> Activities { get; } = new();
    public List<Grade> Grades { get; } = new();
    public List<Invitation> Invitations { get; } = new();
    
    public abstract void SeedData(GirafDbContext dbContext, UserManager<GirafUser> userManager);

    public void SeedUsers(UserManager<GirafUser> userManager)
    {
        var owner = new GirafUser
        {
            UserName = "owner@email.com",
            Email = "owner@email.com",
            FirstName = "Owner",
            LastName = "Ownerson",
            Organizations = new List<Organization>()
        };
        var ownerResult = userManager.CreateAsync(owner, "Password123!").GetAwaiter().GetResult();
        Users.Add("owner", ownerResult.Succeeded ? owner : null);
        userManager.AddClaimAsync(owner, new Claim(ClaimTypes.Name, owner.UserName));
        userManager.AddClaimAsync(owner, new Claim(JwtRegisteredClaimNames.Sub, owner.Id));
        
        var admin = new GirafUser
        {
            UserName = "admin@email.com",
            Email = "admin@email.com",
            FirstName = "Admin",
            LastName = "Adminson",
            Organizations = new List<Organization>()
        };
        var adminResult = userManager.CreateAsync(admin, "Password123!").GetAwaiter().GetResult();
        Users.Add("admin", adminResult.Succeeded ? admin : null);
        userManager.AddClaimAsync(admin, new Claim(ClaimTypes.Name, admin.UserName));
        userManager.AddClaimAsync(admin, new Claim(JwtRegisteredClaimNames.Sub, admin.Id));
        
        var member = new GirafUser
        {
            UserName = "member@email.com",
            Email = "member@email.com",
            FirstName = "Member",
            LastName = "Memberson",
            Organizations = new List<Organization>()
        };
        var memberResult = userManager.CreateAsync(member, "Password123!").GetAwaiter().GetResult();
        Users.Add("member", memberResult.Succeeded ? member : null);
        userManager.AddClaimAsync(member, new Claim(ClaimTypes.Name, member.UserName));
        userManager.AddClaimAsync(member, new Claim(JwtRegisteredClaimNames.Sub, member.Id));
    }

    public void SeedSingleUser(UserManager<GirafUser> userManager)
    {
        var user = new GirafUser
        {
            UserName = "user@email.com",
            Email = "user@email.com",
            FirstName = "User",
            LastName = "Userson",
            Organizations = new List<Organization>()
        };
        var ownerResult = userManager.CreateAsync(user, "Password123!").GetAwaiter().GetResult();
        Users.Add("user", ownerResult.Succeeded ? user : null);
        userManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, user.UserName));
        userManager.AddClaimAsync(user, new Claim(JwtRegisteredClaimNames.Sub, user.Id));
    }

    public void SeedOrganization(GirafDbContext dbContext,
                                 UserManager<GirafUser> userManager,
                                 GirafUser owner, 
                                 List<GirafUser> admins, 
                                 List<GirafUser> members)
    {
        var organization = new Organization
        {
            Id = Organizations.Count + 1,
            Name = "Test Organization",
            Users = new List<GirafUser>(),
            Citizens = new List<Citizen>(),
            Grades = new List<Grade>()
        };
        dbContext.Organizations.Add(organization);
        
        userManager.AddClaimAsync(owner, new Claim("OrgOwner", organization.Id.ToString()));
        userManager.AddClaimAsync(owner, new Claim("OrgAdmin", organization.Id.ToString()));
        userManager.AddClaimAsync(owner, new Claim("OrgMember", organization.Id.ToString()));
        organization.Users.Add(owner);

        if (admins.Any())
        {
            foreach (var admin in admins)
            {
                userManager.AddClaimAsync(admin, new Claim("OrgAdmin", organization.Id.ToString()));
                userManager.AddClaimAsync(admin, new Claim("OrgMember", organization.Id.ToString()));
                organization.Users.Add(admin);
            }
        }

        if (members.Any())
        {
            foreach (var member in members)
            {
                userManager.AddClaimAsync(member, new Claim("OrgMember", organization.Id.ToString()));
                organization.Users.Add(member);
            }
        }

        foreach (var user in organization.Users)
        {
            user.Organizations.Add(organization);
        }
        
        dbContext.SaveChanges();
        Organizations.Add(organization);
    }

    public void SeedCitizens(GirafDbContext dbContext, Organization organization)
    {
        var citizens = new List<Citizen>();
        citizens.Add(new Citizen
        {
            Id = 1,
            FirstName = "Anders",
            LastName = "And",
            Organization = organization,
            Activities = new List<Activity>()
        });
        
        citizens.Add(new Citizen
        {
            Id = 2,
            FirstName = "Rasmus",
            LastName = "Klump",
            Organization = organization,
            Activities = new List<Activity>()
        });
        
        citizens.Add(new Citizen
        {
            Id = 3,
            FirstName = "Bjørnen",
            LastName = "Bjørn",
            Organization = organization,
            Activities = new List<Activity>()
        });

        foreach (var citizen in citizens)
        {
            organization.Citizens.Add(citizen);
            citizen.Organization = organization;
            dbContext.Citizens.Add(citizen);
            Citizens.Add(citizen);
        }
    }

    public void SeedPictogram(GirafDbContext dbContext, Organization organization)
    {
        var pictogram = new Pictogram
        {
            PictogramName = "Test Pictogram",
            PictogramUrl = "https://pictogram.com/pictogram.jpg",
            Id = Pictograms.Count + 1,
            OrganizationId = organization.Id
        };
        dbContext.Pictograms.Add(pictogram);
        dbContext.SaveChanges();
        Pictograms.Add(pictogram);
    }

    public void SeedCitizenActivity(GirafDbContext dbContext, int citizenId, Pictogram pictogram)
    {
        var activity = new Activity
        {
            Id = Activities.Count + 1,
            Date = DateOnly.FromDateTime(DateTime.Now),
            StartTime = TimeOnly.FromDateTime(DateTime.Now),
            EndTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(1)),
            IsCompleted = false,
            Pictogram = pictogram
        };

        var citizen = dbContext.Citizens.Find(citizenId);
        citizen.Activities.Add(activity);
        dbContext.Add(activity);
        dbContext.SaveChanges();
        Activities.Add(activity);
    }
    
    public void SeedGradeActivity(GirafDbContext dbContext, int gradeId, Pictogram pictogram)
    {
        var activity = new Activity
        {
            Id = Activities.Count + 1,
            Date = DateOnly.FromDateTime(DateTime.Now),
            StartTime = TimeOnly.FromDateTime(DateTime.Now),
            EndTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(1)),
            IsCompleted = false,
            Pictogram = pictogram
        };

        var grade = dbContext.Grades.Find(gradeId);
        grade.Activities.Add(activity);
        dbContext.Add(activity);
        dbContext.SaveChanges();
        Activities.Add(activity);
    }

    public void SeedGrade(GirafDbContext dbContext, Organization organization)
    {
        var grade = new Grade
        {
            Id = Grades.Count + 1,
            Name = "Test Grade",
            Citizens = new List<Citizen>(),
            Activities = new List<Activity>()
        };
        
        dbContext.Grades.Add(grade);
        var org = dbContext.Organizations.Find(organization.Id);
        org.Grades.Add(grade);
        dbContext.SaveChanges();
        Grades.Add(grade);
    }

    public void SeedInvitation(GirafDbContext dbContext,
        int orgId,
        String senderId,
        String receiverId)
    {
        var invitation = new Invitation
        {
            Id = Invitations.Count + 1,
            OrganizationId = orgId,
            ReceiverId = receiverId,
            SenderId = senderId
        };
        
        dbContext.Invitations.Add(invitation);
        dbContext.SaveChanges();
        Invitations.Add(invitation);
    }
}