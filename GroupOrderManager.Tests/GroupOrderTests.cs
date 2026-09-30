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
    public void Close_WhenAlreadyClosed_ThrowsDomainException()
    {
        var groupOrder = new GroupOrder(Guid.NewGuid(), "Test Order", DateTime.UtcNow.AddDays(7));
        groupOrder.Close();

        Assert.Throws<DomainException>(() => groupOrder.Close());
    }

    [Fact]
    public void EnsureOpen_WhenOpen_DoesNotThrow()
    {
        var groupOrder = new GroupOrder(Guid.NewGuid(), "Test Order", DateTime.UtcNow.AddDays(7));

        var exception = Record.Exception(() => groupOrder.EnsureOpen());

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureOpen_WhenClosed_ThrowsDomainException()
    {
        var groupOrder = new GroupOrder(Guid.NewGuid(), "Test Order", DateTime.UtcNow.AddDays(7));
        groupOrder.Close();

        Assert.Throws<DomainException>(() => groupOrder.EnsureOpen());
    }
}
