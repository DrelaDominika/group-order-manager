namespace GroupOrderManager.Application.GroupOrderItems;

public record AddGroupOrderItemRequest(Guid GroupOrderId, string Name, decimal Price, int QuantityAvailable);