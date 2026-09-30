using GroupOrderManager.Application.Auth;
using GroupOrderManager.Infrastructure.Persistence;
using GroupOrderManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GroupOrderManager.Tests;

public class AuthServiceTests
{
    private static AuthService CreateService(out GomDbContext dbContext)
    {
        var options = new DbContextOptionsBuilder<GomDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        dbContext = new GomDbContext(options);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-only-signing-key-at-least-32-characters-long",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            })
            .Build();

        return new AuthService(dbContext, configuration);
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_CreatesUserWithHashedPassword()
    {
        var service = CreateService(out var dbContext);

        var id = await service.RegisterAsync(new RegisterRequest("test@example.com", "Password123!"));

        var user = await dbContext.Users.FindAsync(id);
        Assert.NotNull(user);
        Assert.Equal("test@example.com", user.Email);
        Assert.NotEqual("Password123!", user.PasswordHash);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        var service = CreateService(out _);
        await service.RegisterAsync(new RegisterRequest("dup@example.com", "Password123!"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegisterAsync(new RegisterRequest("dup@example.com", "AnotherPassword1!")));
    }

    [Fact]
    public async Task LoginAsync_WithCorrectPassword_ReturnsToken()
    {
        var service = CreateService(out _);
        await service.RegisterAsync(new RegisterRequest("login@example.com", "Password123!"));

        var result = await service.LoginAsync(new LoginRequest("login@example.com", "Password123!"));

        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorizedAccessException()
    {
        var service = CreateService(out _);
        await service.RegisterAsync(new RegisterRequest("wrongpw@example.com", "Password123!"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest("wrongpw@example.com", "WrongPassword!")));
    }

    [Fact]
    public async Task LoginAsync_WithNonexistentEmail_ThrowsUnauthorizedAccessException()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest("nobody@example.com", "Whatever123!")));
    }
}