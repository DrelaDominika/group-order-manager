namespace GroupOrderManager.Application.Claims;

public record ClaimItemRequest(Guid GroupOrderItemId, Guid ParticipantId, int Quantity);