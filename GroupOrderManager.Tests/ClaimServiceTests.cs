using GroupOrderManager.Application.Claims;
using GroupOrderManager.Application.Common.Exceptions;
using GroupOrderManager.Domain;
using GroupOrderManager.Infrastructure.Persistence;
using GroupOrderManager.Infrastructure.Services;
using Xunit;

namespace GroupOrderManager.Tests;

public class ClaimServiceTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    // One open order with one item (10 available) and one participant.
    private static async Task<(GomDbContext Db, GroupOrder Order, GroupOrderItem Item, Participant Participant)> SeedAsync()
    {
        var db = TestDb.Create();
        var order = new GroupOrder(OwnerId, "Album GO", DateTime.UtcNow.AddDays(7));
        var item = new GroupOrderItem(order.Id, "Album", 100m, 10);
        var participant = new Participant(order.Id, "Nicholas", "nicholas@example.com");
        db.AddRange(order, item, participant);
        await db.SaveChangesAsync();
        return (db, order, item, participant);
    }

    [Fact]
    public async Task ClaimAsync_WithValidRequest_CreatesClaimAndIncreasesQuantityClaimed()
    {
        var (db, _, item, participant) = await SeedAsync();
        var service = new ClaimService(db);

        var claimId = await service.ClaimAsync(new ClaimItemRequest(item.Id, participant.Id, 3));

        var claim = await db.Claims.FindAsync(claimId);
        Assert.NotNull(claim);
        Assert.Equal(3, claim.Quantity);
        Assert.Equal(3, item.QuantityClaimed);
    }

    [Fact]
    public async Task ClaimAsync_WhenItemDoesNotExist_ThrowsNotFoundException()
    {
        var (db, _, _, participant) = await SeedAsync();
        var service = new ClaimService(db);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.ClaimAsync(new ClaimItemRequest(Guid.NewGuid(), participant.Id, 1)));
    }

    [Fact]
    public async Task ClaimAsync_WhenOrderIsClosed_ThrowsDomainExceptionAndClaimsNothing()
    {
        var (db, order, item, participant) = await SeedAsync();
        order.Close();
        await db.SaveChangesAsync();
        var service = new ClaimService(db);

        await Assert.ThrowsAsync<DomainException>(() =>
            service.ClaimAsync(new ClaimItemRequest(item.Id, participant.Id, 1)));
        Assert.Equal(0, item.QuantityClaimed);
    }

    [Fact]
    public async Task ClaimAsync_WhenParticipantBelongsToAnotherOrder_ThrowsNotFoundException()
    {
        var (db, _, item, _) = await SeedAsync();
        var otherOrder = new GroupOrder(OwnerId, "Other GO", DateTime.UtcNow.AddDays(7));
        var outsider = new Participant(otherOrder.Id, "Outsider", "outsider@example.com");
        db.AddRange(otherOrder, outsider);
        await db.SaveChangesAsync();
        var service = new ClaimService(db);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.ClaimAsync(new ClaimItemRequest(item.Id, outsider.Id, 1)));
    }

    [Fact]
    public async Task MarkAsPaidAsync_ByOwner_MarksClaimPaid()
    {
        var (db, _, item, participant) = await SeedAsync();
        var service = new ClaimService(db);
        var claimId = await service.ClaimAsync(new ClaimItemRequest(item.Id, participant.Id, 1));

        await service.MarkAsPaidAsync(claimId, OwnerId);

        var claim = await db.Claims.FindAsync(claimId);
        Assert.True(claim!.IsPaid);
    }

    [Fact]
    public async Task MarkAsPaidAsync_ByNonOwner_ThrowsNotFoundExceptionAndClaimStaysUnpaid()
    {
        var (db, _, item, participant) = await SeedAsync();
        var service = new ClaimService(db);
        var claimId = await service.ClaimAsync(new ClaimItemRequest(item.Id, participant.Id, 1));

        await Assert.ThrowsAsync<NotFoundException>(() => service.MarkAsPaidAsync(claimId, Guid.NewGuid()));

        var claim = await db.Claims.FindAsync(claimId);
        Assert.False(claim!.IsPaid);
    }
}
