using System.IdentityModel.Tokens.Jwt;
using System.Text;
using GroupOrderManager.Application.Auth;
using GroupOrderManager.Application.Common.Exceptions;
using GroupOrderManager.Domain;
using GroupOrderManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using SecurityClaim = System.Security.Claims.Claim;

namespace GroupOrderManager.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly GomDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthService(GomDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<Guid> RegisterAsync(RegisterRequest request)
    {
        var email = NormalizeEmail(request.Email);

        var existing = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existing is not null)
            throw new ConflictException("A user with this email already exists.");

        // The default PasswordHasher ignores its user argument (it exists so custom hashers can use
        // per-user data). We need the hash before we can build the real User, whose constructor
        // requires one, so we pass a throwaway instance.
        var tempUser = new User(email, "placeholder");
        var hash = _passwordHasher.HashPassword(tempUser, request.Password);

        var user = new User(email, hash);
        _dbContext.Users.Add(user);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Two registrations for the same email raced past the check above; the unique index caught it.
            throw new ConflictException("A user with this email already exists.");
        }

        return user.Id;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = NormalizeEmail(request.Email);

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = GenerateJwt(user);
        return new AuthResponse(token);
    }

    // Emails are case-insensitive in practice: "Domi@x.com" and "domi@x.com" must be the same account.
    private static string NormalizeEmail(string? email) => (email ?? string.Empty).Trim().ToLowerInvariant();

    private string GenerateJwt(User user)
    {
        var claims = new SecurityClaim[]
        {
            new SecurityClaim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new SecurityClaim(JwtRegisteredClaimNames.Email, user.Email)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(4),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
