namespace GroupOrderManager.Application.GroupOrders;

public record ParticipantAmountOwedResponse(Guid ParticipantId, string ParticipantName, decimal AmountOwed);