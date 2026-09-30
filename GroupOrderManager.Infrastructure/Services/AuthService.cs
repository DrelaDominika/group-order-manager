using System.IdentityModel.Tokens.Jwt;
using System.Text;
using GroupOrderManager.Application.Auth;
using GroupOrderManager.Domain;
using GroupOrderManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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
        var existing = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existing is not null)
            throw new InvalidOperationException("A user with this email already exists.");

        // PasswordHasher needs a User instance to hash against, but doesn't use its Id/Email —
        // it only reads the object identity for the algorithm. We pass a throwaway hash first,
        // then build the real User with the computed hash.
        var tempUser = new User(request.Email, "placeholder");
        var hash = _passwordHasher.HashPassword(tempUser, request.Password);

        var user = new User(request.Email, hash);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return user.Id;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user is null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = GenerateJwt(user);
        return new AuthResponse(token);
    }

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