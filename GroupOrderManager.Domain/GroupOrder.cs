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
}

public enum GroupOrderStatus
{
    Open,
    Closed
}