using GroupOrderManager.Domain;
using Xunit;

namespace GroupOrderManager.Tests;

public class GroupOrderItemTests
{
    [Fact]
    public void ClaimFor_WhenQuantityAvailable_IncreasesQuantityClaimed()
    {
        // Arrange
        var item = new GroupOrderItem(Guid.NewGuid(), "Album", 100m, 10);
        var participantId = Guid.NewGuid();

        // Act
        var claim = item.ClaimFor(participantId, 3);

        // Assert
        Assert.Equal(3, claim.Quantity);
        Assert.Equal(participantId, claim.ParticipantId);
        Assert.Equal(item.Id, claim.GroupOrderItemId);
    }

    [Fact]
    public void ClaimFor_WhenQuantityExceedsAvailable_ThrowsInvalidOperationException()
    {
        // Arrange
        var item = new GroupOrderItem(Guid.NewGuid(), "Album", 100m, 10);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => item.ClaimFor(Guid.NewGuid(), 11));
    }

    [Fact]
    public void ClaimFor_WhenExactQuantityAvailable_ShouldSucceed()
    {
        // Arrange
        var item = new GroupOrderItem(Guid.NewGuid(), "Album", 100m, 10);
        var participantId = Guid.NewGuid();

        // Act
        var claim = item.ClaimFor(participantId, 10);

        // Assert
        Assert.Equal(10, claim.Quantity);
        Assert.Equal(participantId, claim.ParticipantId);
        Assert.Equal(item.Id, claim.GroupOrderItemId);
        Assert.Equal(10, item.QuantityClaimed);
    }

    [Fact]
    public void ClaimFor_WhenSequentialClaimsAreMade_SecondClaimRespectsFirstClaim()
    {
        // Arrange
        var item = new GroupOrderItem(Guid.NewGuid(), "Album", 100m, 10);
        var firstParticipantId = Guid.NewGuid();
        var secondParticipantId = Guid.NewGuid();

        // Act
        var claim = item.ClaimFor(firstParticipantId, 6);
        var secondClaim = item.ClaimFor(secondParticipantId, 4);

        // Assert
        Assert.Equal(6, claim.Quantity);
        Assert.Equal(firstParticipantId, claim.ParticipantId);
        Assert.Equal(4, secondClaim.Quantity);
        Assert.Equal(secondParticipantId, secondClaim.ParticipantId);
        Assert.Equal(item.Id, claim.GroupOrderItemId);
        Assert.Equal(10, item.QuantityClaimed);
        Assert.Throws<InvalidOperationException>(() => item.ClaimFor(Guid.NewGuid(), 1));
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(100, -1)]
    public void Constructor_WhenPriceOrQuantityIsNegative_ThrowsArgumentException(decimal price, int quantityAvailable)
    {
        Assert.Throws<ArgumentException>(() => new GroupOrderItem(Guid.NewGuid(), "Album", price, quantityAvailable));
    }
}