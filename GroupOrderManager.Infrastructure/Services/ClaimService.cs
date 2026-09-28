using GroupOrderManager.Application.Claims;
using GroupOrderManager.Infrastructure.Persistence;

namespace GroupOrderManager.Infrastructure.Services;

public class ClaimService : IClaimService
{
    private readonly GomDbContext _dbContext;

    public ClaimService(GomDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> ClaimAsync(ClaimItemRequest request)
    {
        var item = await _dbContext.GroupOrderItems.FindAsync(request.GroupOrderItemId);
        if (item is null)
            throw new InvalidOperationException($"GroupOrderItem {request.GroupOrderItemId} does not exist.");

        var claim = item.ClaimFor(request.ParticipantId, request.Quantity);

        _dbContext.Claims.Add(claim);
        await _dbContext.SaveChangesAsync();

        return claim.Id;
    }

    public async Task MarkAsPaidAsync(Guid claimId)
    {
        var claim = await _dbContext.Claims.FindAsync(claimId);
        if (claim is null)
            throw new InvalidOperationException($"Claim {claimId} does not exist.");

        claim.MarkAsPaid();
        await _dbContext.SaveChangesAsync();
    }

    public async Task MarkAsUnpaidAsync(Guid claimId)
    {
        var claim = await _dbContext.Claims.FindAsync(claimId);
        if (claim is null)
            throw new InvalidOperationException($"Claim {claimId} does not exist.");

        claim.MarkAsUnpaid();
        await _dbContext.SaveChangesAsync();
    }
}