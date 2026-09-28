using GroupOrderManager.Application.Participants;
using GroupOrderManager.Domain;
using GroupOrderManager.Infrastructure.Persistence;

namespace GroupOrderManager.Infrastructure.Services;

public class ParticipantService : IParticipantService
{
    private readonly GomDbContext _dbContext;

    public ParticipantService(GomDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> AddAsync(AddParticipantRequest request)
    {
        var groupOrder = await _dbContext.GroupOrders.FindAsync(request.GroupOrderId);
        if (groupOrder is null)
            throw new InvalidOperationException($"GroupOrder {request.GroupOrderId} does not exist.");

        var participant = new Participant(request.GroupOrderId, request.Name, request.ContactInfo);

        _dbContext.Participants.Add(participant);
        await _dbContext.SaveChangesAsync();

        return participant.Id;
    }
}