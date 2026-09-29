using GroupOrderManager.Application.GroupOrders;
using GroupOrderManager.Domain;
using GroupOrderManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

    public async Task<GroupOrderDetailsResponse?> GetByIdAsync(Guid id)
    {
        var groupOrder = await _dbContext.GroupOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (groupOrder is null)
            return null;

        return new GroupOrderDetailsResponse(
            groupOrder.Id,
            groupOrder.Title,
            groupOrder.Description,
            groupOrder.Deadline,
            groupOrder.Status.ToString(),
            groupOrder.Items.Select(i => new GroupOrderItemResponse(i.Id, i.Name, i.Price, i.QuantityAvailable, i.QuantityClaimed)).ToList());
    }

    public async Task CloseAsync(Guid id)
    {
        var groupOrder = await _dbContext.GroupOrders.FindAsync(id);
        if (groupOrder is null)
            throw new InvalidOperationException($"GroupOrder {id} does not exist.");

        groupOrder.Close();
        await _dbContext.SaveChangesAsync();
    }
}