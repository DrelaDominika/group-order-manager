namespace GroupOrderManager.Domain;

public class GroupOrderItem
{
    public Guid Id { get; private set; }
    public Guid GroupOrderId { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal Price { get; private set; }
    public int QuantityAvailable { get; private set; }
    public int QuantityClaimed { get; private set; }

    private GroupOrderItem() { } // for EF Core

    public GroupOrderItem(Guid groupOrderId, string name, decimal price, int quantityAvailable)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));
        if (quantityAvailable < 0)
            throw new ArgumentException("Quantity cannot be negative.", nameof(quantityAvailable));

        Id = Guid.NewGuid();
        GroupOrderId = groupOrderId;
        Name = name;
        Price = price;
        QuantityAvailable = quantityAvailable;
        QuantityClaimed = 0;
    }

    public bool CanClaim(int quantity) => QuantityClaimed + quantity <= QuantityAvailable;

    public void Claim(int quantity)
    {
        if (!CanClaim(quantity))
            throw new InvalidOperationException("Not enough quantity available to claim.");

        QuantityClaimed += quantity;
    }
}