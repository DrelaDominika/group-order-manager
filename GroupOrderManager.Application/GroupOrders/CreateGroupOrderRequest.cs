namespace GroupOrderManager.Application.GroupOrders;

public record CreateGroupOrderRequest(string Title, DateTime Deadline, string? Description);