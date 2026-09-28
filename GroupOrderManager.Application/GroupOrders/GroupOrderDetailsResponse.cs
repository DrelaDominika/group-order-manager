namespace GroupOrderManager.Application.GroupOrders;

public record GroupOrderDetailsResponse(
    Guid Id,
    string Title,
    string? Description,
    DateTime Deadline,
    string Status,
    List<GroupOrderItemResponse> Items);

public record GroupOrderItemResponse(
    Guid Id,
    string Name,
    decimal Price,
    int QuantityAvailable,
    int QuantityClaimed);