namespace GroupOrderManager.Application.GroupOrderItems;

public interface IGroupOrderItemService
{
    Task<Guid> AddAsync(AddGroupOrderItemRequest request);
}