namespace GroupOrderManager.Domain;

public class Participant
{
    public Guid Id { get; private set; }
    public Guid GroupOrderId { get; private set; }
    public string Name { get; private set; } = null!;
    public string ContactInfo { get; private set; } = null!;

    private Participant() { } // for EF Core

    public Participant(Guid groupOrderId, string name, string contactInfo)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(contactInfo))
            throw new ArgumentException("Contact info is required.", nameof(contactInfo));

        Id = Guid.NewGuid();
        GroupOrderId = groupOrderId;
        Name = name;
        ContactInfo = contactInfo;
    }
}