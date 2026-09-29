using GroupOrderManager.Domain;
using Xunit;

namespace GroupOrderManager.Tests;

public class GroupOrderTests
{
    [Fact]
    public void Close_WhenOpen_SetsStatusToClosed()
    {
        var groupOrder = new GroupOrder(Guid.NewGuid(), "Test Order", DateTime.UtcNow.AddDays(7));

        groupOrder.Close();

        Assert.Equal(GroupOrderStatus.Closed, groupOrder.Status);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ThrowsInvalidOperationException()
    {
        var groupOrder = new GroupOrder(Guid.NewGuid(), "Test Order", DateTime.UtcNow.AddDays(7));
        groupOrder.Close();

        Assert.Throws<InvalidOperationException>(() => groupOrder.Close());
    }
}