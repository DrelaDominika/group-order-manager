namespace GroupOrderManager.Application.Claims;

public interface IClaimService
{
    Task<Guid> ClaimAsync(ClaimItemRequest request);
    Task MarkAsPaidAsync(Guid claimId, Guid currentUserId);
    Task MarkAsUnpaidAsync(Guid claimId, Guid currentUserId);
}
