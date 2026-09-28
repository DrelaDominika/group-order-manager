namespace GroupOrderManager.Application.GroupOrders;

public record CreateGroupOrderRequest(Guid OwnerId, string Title, DateTime Deadline, string? Description);