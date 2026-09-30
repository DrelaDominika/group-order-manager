using GroupOrderManager.Application.Claims;
using GroupOrderManager.Application.Common.Exceptions;
using GroupOrderManager.Domain;
using GroupOrderManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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
        var item = await _dbContext.GroupOrderItems.FindAsync(request.GroupOrderItemId)
            ?? throw new NotFoundException($"Item {request.GroupOrderItemId} was not found.");

        var groupOrder = await _dbContext.GroupOrders.FindAsync(item.GroupOrderId)
            ?? throw new NotFoundException($"GroupOrder {item.GroupOrderId} was not found.");

        groupOrder.EnsureOpen();

        // The participant must exist and belong to the same group order as the item.
        var participant = await _dbContext.Participants.FindAsync(request.ParticipantId);
        if (participant is null || participant.GroupOrderId != item.GroupOrderId)
            throw new NotFoundException($"Participant {request.ParticipantId} was not found in this group order.");

        var claim = item.ClaimFor(request.ParticipantId, request.Quantity);
        _dbContext.Claims.Add(claim);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Someone else updated this item (e.g. claimed the last unit) between our read and our write.
            // The xmin check made our UPDATE match 0 rows, EF rolled the transaction back — nothing was oversold.
            throw new ConflictException("This item was just updated by someone else. Please refresh and try again.");
        }

        return claim.Id;
    }

    public async Task MarkAsPaidAsync(Guid claimId, Guid currentUserId)
    {
        var claim = await GetClaimOwnedByAsync(claimId, currentUserId);
        claim.MarkAsPaid();
        await _dbContext.SaveChangesAsync();
    }

    public async Task MarkAsUnpaidAsync(Guid claimId, Guid currentUserId)
    {
        var claim = await GetClaimOwnedByAsync(claimId, currentUserId);
        claim.MarkAsUnpaid();
        await _dbContext.SaveChangesAsync();
    }

    // Walks claim -> item -> group order and checks the order's owner.
    // Missing at any step, or not the owner: same 404.
    private async Task<Claim> GetClaimOwnedByAsync(Guid claimId, Guid currentUserId)
    {
        var claim = await _dbContext.Claims.FindAsync(claimId);
        if (claim is null)
            throw new NotFoundException($"Claim {claimId} was not found.");

        var item = await _dbContext.GroupOrderItems.FindAsync(claim.GroupOrderItemId);
        var groupOrder = item is null ? null : await _dbContext.GroupOrders.FindAsync(item.GroupOrderId);

        if (groupOrder is null || groupOrder.OwnerId != currentUserId)
            throw new NotFoundException($"Claim {claimId} was not found.");

        return claim;
    }
}
