namespace GroupOrderManager.Application.GroupOrders;

public interface IGroupOrderService
{
    Task<Guid> CreateAsync(CreateGroupOrderRequest request);
    Task<GroupOrderDetailsResponse?> GetByIdAsync(Guid id);
    Task CloseAsync(Guid id);
    Task<List<ParticipantAmountOwedResponse>> GetAmountOwedAsync(Guid groupOrderId);
}