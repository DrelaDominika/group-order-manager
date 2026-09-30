namespace GroupOrderManager.Domain;

public class GroupOrder
{
    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTime Deadline { get; private set; }
    public GroupOrderStatus Status { get; private set; }

    private GroupOrder() { } // for EF Core

    public GroupOrder(Guid ownerId, string title, DateTime deadline, string? description = null)
    {
        Id = Guid.NewGuid();
        OwnerId = ownerId;
        Title = title;
        Deadline = deadline;
        Description = description;
        Status = GroupOrderStatus.Open;
    }

    private readonly List<GroupOrderItem> _items = new();
    public IReadOnlyCollection<GroupOrderItem> Items => _items.AsReadOnly();

    public void Close()
    {
        if (Status == GroupOrderStatus.Closed)
            throw new DomainException("GroupOrder is already closed.");

        Status = GroupOrderStatus.Closed;
    }

    /// <summary>
    /// Guard used before anything changes the order's contents (adding items, joining, claiming).
    /// A closed order is frozen.
    /// </summary>
    public void EnsureOpen()
    {
        if (Status == GroupOrderStatus.Closed)
            throw new DomainException("This group order is closed.");
    }
}

public enum GroupOrderStatus
{
    Open,
    Closed
}
