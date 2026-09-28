namespace GroupOrderManager.Application.Claims;

public interface IClaimService
{
    Task<Guid> ClaimAsync(ClaimItemRequest request);
}