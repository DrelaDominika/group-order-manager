using GroupOrderManager.Application.Common.Exceptions;
using GroupOrderManager.Application.Participants;
using GroupOrderManager.Domain;
using GroupOrderManager.Infrastructure.Services;
using Xunit;

namespace GroupOrderManager.Tests;

public class ParticipantServiceTests
{
    [Fact]
    public async Task AddAsync_WhenOrderDoesNotExist_ThrowsNotFoundException()
    {
        var service = new ParticipantService(TestDb.Create());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.AddAsync(new AddParticipantRequest(Guid.NewGuid(), "Nicholas", "nicholas@example.com")));
    }

    [Fact]
    public async Task AddAsync_WhenOrderIsClosed_ThrowsDomainException()
    {
        var db = TestDb.Create();
        var order = new GroupOrder(Guid.NewGuid(), "Album GO", DateTime.UtcNow.AddDays(7));
        order.Close();
        db.GroupOrders.Add(order);
        await db.SaveChangesAsync();
        var service = new ParticipantService(db);

        await Assert.ThrowsAsync<DomainException>(() =>
            service.AddAsync(new AddParticipantRequest(order.Id, "Nicholas", "nicholas@example.com")));
    }
}
