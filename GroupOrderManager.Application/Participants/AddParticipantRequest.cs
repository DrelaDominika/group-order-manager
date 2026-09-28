namespace GroupOrderManager.Application.Participants;

public record AddParticipantRequest(Guid GroupOrderId, string Name, string ContactInfo);