using GroupOrderManager.Application.Common.Exceptions;
using GroupOrderManager.Application.GroupOrderItems;
using GroupOrderManager.Domain;
using GroupOrderManager.Infrastructure.Services;
using Xunit;

namespace GroupOrderManager.Tests;

public class GroupOrderItemServiceTests
{
    [Fact]
    public async Task AddAsync_ByOwner_AddsItem()
    {
        var db = TestDb.Create();
        var ownerId = Guid.NewGuid();
        var order = new GroupOrder(ownerId, "Album GO", DateTime.UtcNow.AddDays(7));
        db.GroupOrders.Add(order);
        await db.SaveChangesAsync();
        var service = new GroupOrderItemService(db);

        var itemId = await service.AddAsync(new AddGroupOrderItemRequest(order.Id, "Album", 100m, 10), ownerId);

        var item = await db.GroupOrderItems.FindAsync(itemId);
        Assert.NotNull(item);
        Assert.Equal(order.Id, item.GroupOrderId);
    }

    [Fact]
    public async Task AddAsync_ByNonOwner_ThrowsNotFoundException()
    {
        var db = TestDb.Create();
        var order = new GroupOrder(Guid.NewGuid(), "Album GO", DateTime.UtcNow.AddDays(7));
        db.GroupOrders.Add(order);
        await db.SaveChangesAsync();
        var service = new GroupOrderItemService(db);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.AddAsync(new AddGroupOrderItemRequest(order.Id, "Album", 100m, 10), Guid.NewGuid()));
    }

    [Fact]
    public async Task AddAsync_WhenOrderIsClosed_ThrowsDomainException()
    {
        var db = TestDb.Create();
        var ownerId = Guid.NewGuid();
        var order = new GroupOrder(ownerId, "Album GO", DateTime.UtcNow.AddDays(7));
        order.Close();
        db.GroupOrders.Add(order);
        await db.SaveChangesAsync();
        var service = new GroupOrderItemService(db);

        await Assert.ThrowsAsync<DomainException>(() =>
            service.AddAsync(new AddGroupOrderItemRequest(order.Id, "Album", 100m, 10), ownerId));
    }
}
