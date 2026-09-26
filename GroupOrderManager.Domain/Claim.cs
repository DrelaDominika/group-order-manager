namespace GroupOrderManager.Domain;

public class Claim
{
    public Guid Id { get; private set; }
    public Guid ParticipantId { get; private set; }
    public Guid GroupOrderItemId { get; private set; }
    public int Quantity { get; private set; }
    public bool IsPaid { get; private set; }

    private Claim() { } // for EF Core

    public Claim(Guid participantId, Guid groupOrderItemId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        Id = Guid.NewGuid();
        ParticipantId = participantId;
        GroupOrderItemId = groupOrderItemId;
        Quantity = quantity;
        IsPaid = false;
    }

    public void MarkAsPaid()
    {
        IsPaid = true;
    }

    public void MarkAsUnpaid()
    {
        IsPaid = false;
    }
}