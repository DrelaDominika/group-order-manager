namespace GroupOrderManager.Application.Claims;

public interface IClaimService
{
    Task<Guid> ClaimAsync(ClaimItemRequest request);
    Task MarkAsPaidAsync(Guid claimId);
    Task MarkAsUnpaidAsync(Guid claimId);
}