using GroupOrderManager.Application.GroupOrders;
using GroupOrderManager.Domain;
using GroupOrderManager.Infrastructure.Persistence;

namespace GroupOrderManager.Infrastructure.Services;

public class GroupOrderService : IGroupOrderService
{
    private readonly GomDbContext _dbContext;

    public GroupOrderService(GomDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> CreateAsync(CreateGroupOrderRequest request)
    {
        var groupOrder = new GroupOrder(request.OwnerId, request.Title, request.Deadline, request.Description);

        _dbContext.GroupOrders.Add(groupOrder);
        await _dbContext.SaveChangesAsync();

        return groupOrder.Id;
    }
}