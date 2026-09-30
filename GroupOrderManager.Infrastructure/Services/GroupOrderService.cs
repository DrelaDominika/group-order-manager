using GroupOrderManager.Application.Common.Exceptions;
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

    public async Task<Guid> CreateAsync(CreateGroupOrderRequest request, Guid ownerId)
    {
        var groupOrder = new GroupOrder(ownerId, request.Title, request.Deadline, request.Description);

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

    public async Task CloseAsync(Guid id, Guid currentUserId)
    {
        var groupOrder = await _dbContext.GroupOrders.FindAsync(id);

        // Same 404 whether the order doesn't exist or belongs to someone else,
        // so the API never confirms that someone else's order exists.
        if (groupOrder is null || groupOrder.OwnerId != currentUserId)
            throw new NotFoundException($"GroupOrder {id} was not found.");

        groupOrder.Close();
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<ParticipantAmountOwedResponse>> GetAmountOwedAsync(Guid groupOrderId)
    {
        var groupOrder = await _dbContext.GroupOrders.FindAsync(groupOrderId);
        if (groupOrder is null)
            throw new NotFoundException($"GroupOrder {groupOrderId} was not found.");

        var result = await (
            from claim in _dbContext.Claims
            join item in _dbContext.GroupOrderItems on claim.GroupOrderItemId equals item.Id
            join participant in _dbContext.Participants on claim.ParticipantId equals participant.Id
            where item.GroupOrderId == groupOrderId
            group item.Price * claim.Quantity by new { participant.Id, participant.Name } into g
            select new ParticipantAmountOwedResponse(g.Key.Id, g.Key.Name, g.Sum())
        ).ToListAsync();

        return result;
    }
}
