using GroupOrderManager.Application.Common.Exceptions;
using GroupOrderManager.Domain;
using GroupOrderManager.Infrastructure.Services;
using Xunit;

namespace GroupOrderManager.Tests;

public class GroupOrderServiceTests
{
    [Fact]
    public async Task CloseAsync_ByOwner_ClosesOrder()
    {
        var db = TestDb.Create();
        var ownerId = Guid.NewGuid();
        var order = new GroupOrder(ownerId, "Album GO", DateTime.UtcNow.AddDays(7));
        db.GroupOrders.Add(order);
        await db.SaveChangesAsync();
        var service = new GroupOrderService(db);

        await service.CloseAsync(order.Id, ownerId);

        Assert.Equal(GroupOrderStatus.Closed, order.Status);
    }

    [Fact]
    public async Task CloseAsync_ByNonOwner_ThrowsNotFoundExceptionAndOrderStaysOpen()
    {
        var db = TestDb.Create();
        var order = new GroupOrder(Guid.NewGuid(), "Album GO", DateTime.UtcNow.AddDays(7));
        db.GroupOrders.Add(order);
        await db.SaveChangesAsync();
        var service = new GroupOrderService(db);

        await Assert.ThrowsAsync<NotFoundException>(() => service.CloseAsync(order.Id, Guid.NewGuid()));
        Assert.Equal(GroupOrderStatus.Open, order.Status);
    }

    [Fact]
    public async Task CloseAsync_WhenOrderDoesNotExist_ThrowsNotFoundException()
    {
        var service = new GroupOrderService(TestDb.Create());

        await Assert.ThrowsAsync<NotFoundException>(() => service.CloseAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAmountOwedAsync_WhenOrderDoesNotExist_ThrowsNotFoundException()
    {
        var service = new GroupOrderService(TestDb.Create());

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAmountOwedAsync(Guid.NewGuid()));
    }
}
