namespace GroupOrderManager.Application.Participants;

public interface IParticipantService
{
    Task<Guid> AddAsync(AddParticipantRequest request);
}