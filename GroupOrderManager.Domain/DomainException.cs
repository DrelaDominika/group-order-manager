namespace GroupOrderManager.Domain;

/// <summary>
/// Thrown when an operation would break a business rule — for example claiming more than is
/// available, or changing a group order that is already closed. The API maps it to 409 Conflict.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
