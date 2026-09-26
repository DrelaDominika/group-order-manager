using GroupOrderManager.Domain;
using Xunit;

namespace GroupOrderManager.Tests;

public class ClaimTests
{
    [Fact]
    public void Constructor_WithValidQuantity_CreatesUnpaidClaim()
    {
        // Arrange
        var participantId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        // Act
        var claim = new Claim(participantId, itemId, 3);

        // Assert
        Assert.Equal(participantId, claim.ParticipantId);
        Assert.Equal(itemId, claim.GroupOrderItemId);
        Assert.Equal(3, claim.Quantity);
        Assert.False(claim.IsPaid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_WhenQuantityIsNotPositive_ThrowsArgumentException(int invalidQuantity)
    {
        Assert.Throws<ArgumentException>(() => new Claim(Guid.NewGuid(), Guid.NewGuid(), invalidQuantity));
    }
}