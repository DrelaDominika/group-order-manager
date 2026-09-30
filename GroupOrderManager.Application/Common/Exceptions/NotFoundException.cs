namespace GroupOrderManager.Application.Common.Exceptions;

/// <summary>
/// The requested resource doesn't exist — or exists but belongs to someone else.
/// Both cases deliberately look the same to the caller. Mapped to 404.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
