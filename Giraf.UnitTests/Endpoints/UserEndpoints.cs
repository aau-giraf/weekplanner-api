using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using GirafAPI.Entities.Users;
using GirafAPI.Entities.Users.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Xunit;


namespace GirafAPI.UnitTests.Endpoints
{
    private readonly WebApplicationFactory<Program> _factory;

}