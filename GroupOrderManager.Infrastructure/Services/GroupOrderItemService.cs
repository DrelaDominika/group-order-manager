using GroupOrderManager.Application.Common.Exceptions;
using GroupOrderManager.Application.GroupOrderItems;
using GroupOrderManager.Domain;
using GroupOrderManager.Infrastructure.Persistence;

namespace GroupOrderManager.Infrastructure.Services;

public class GroupOrderItemService : IGroupOrderItemService
{
    private readonly GomDbContext _dbContext;

    public GroupOrderItemService(GomDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> AddAsync(AddGroupOrderItemRequest request, Guid currentUserId)
    {
        var groupOrder = await _dbContext.GroupOrders.FindAsync(request.GroupOrderId);

        // Only the owner can add items; non-owners get the same 404 as a missing order.
        if (groupOrder is null || groupOrder.OwnerId != currentUserId)
            throw new NotFoundException($"GroupOrder {request.GroupOrderId} was not found.");

        groupOrder.EnsureOpen();

        var item = new GroupOrderItem(request.GroupOrderId, request.Name, request.Price, request.QuantityAvailable);

        _dbContext.GroupOrderItems.Add(item);
        await _dbContext.SaveChangesAsync();

        return item.Id;
    }
}
