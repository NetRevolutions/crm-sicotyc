using FluentAssertions;
using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Jarasoft.Sicotyc.Infrastructure.Identity;
using Jarasoft.Sicotyc.Test.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Jarasoft.Sicotyc.Test.API.Authentication
{
    public sealed class IdentitySeederTests
        : IClassFixture<CustomWebApplicationFactory>    
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;


        public IdentitySeederTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task IdentitySeeder_ShouldCreateAllApplicationRoles()
        {
            using var scope =
                _factory.Services.CreateScope();

            var roleManager =
                scope.ServiceProvider
                    .GetRequiredService<
                        RoleManager<ApplicationRole>>();

            foreach (var role in ApplicationRoles.All)
            {
                var exists =
                    await roleManager.RoleExistsAsync(role);

                exists
                    .Should()
                    .BeTrue();
            }
        }
    }
}
