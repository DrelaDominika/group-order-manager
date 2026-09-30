namespace GroupOrderManager.Application.GroupOrders;

public interface IGroupOrderService
{
    Task<Guid> CreateAsync(CreateGroupOrderRequest request, Guid ownerId);
    Task<GroupOrderDetailsResponse?> GetByIdAsync(Guid id);
    Task CloseAsync(Guid id, Guid currentUserId);
    Task<List<ParticipantAmountOwedResponse>> GetAmountOwedAsync(Guid groupOrderId);
}
