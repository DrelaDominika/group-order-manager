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

    public async Task<Guid> AddAsync(AddGroupOrderItemRequest request)
    {
        var groupOrder = await _dbContext.GroupOrders.FindAsync(request.GroupOrderId);
        if (groupOrder is null)
            throw new InvalidOperationException($"GroupOrder {request.GroupOrderId} does not exist.");

        var item = new GroupOrderItem(request.GroupOrderId, request.Name, request.Price, request.QuantityAvailable);

        _dbContext.GroupOrderItems.Add(item);
        await _dbContext.SaveChangesAsync();

        return item.Id;
    }
}