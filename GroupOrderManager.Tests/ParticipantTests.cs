using GroupOrderManager.Domain;
using Xunit;

namespace GroupOrderManager.Tests;

public class ParticipantTests
{
    [Fact]
    public void Constructor_WithValidNameAndContactInfo_CreatesParticipant()
    {
        // Arrange
        var groupOrderId = Guid.NewGuid();

        // Act
        var participant = new Participant(groupOrderId, "Nicholas", "nicholas@example.com");

        // Assert
        Assert.Equal(groupOrderId, participant.GroupOrderId);
        Assert.Equal("Nicholas", participant.Name);
        Assert.Equal("nicholas@example.com", participant.ContactInfo);
    }

    [Theory]
    [InlineData(null, "nicholas@example.com")]
    [InlineData("", "nicholas@example.com")]
    [InlineData("   ", "nicholas@example.com")]
    [InlineData("Nicholas", null)]
    [InlineData("Nicholas", "")]
    [InlineData("Nicholas", "   ")]
    public void Constructor_WhenNameOrContactInfoMissing_ThrowsArgumentException(string? name, string? contactInfo)
    {
        Assert.Throws<ArgumentException>(() => new Participant(Guid.NewGuid(), name!, contactInfo!));
    }
}