namespace GroupOrderManager.Application.Auth;

public interface IAuthService
{
    Task<Guid> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}