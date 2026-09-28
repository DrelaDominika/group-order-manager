namespace GroupOrderManager.Application.GroupOrders;

public interface IGroupOrderService
{
    Task<Guid> CreateAsync(CreateGroupOrderRequest request);
}