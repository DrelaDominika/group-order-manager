namespace GroupOrderManager.Application.Common.Exceptions;

/// <summary>
/// The request conflicts with the current state of the data (duplicate email, concurrent update).
/// Mapped to 409.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
