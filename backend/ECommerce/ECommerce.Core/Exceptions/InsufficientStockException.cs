namespace ECommerce.Core.Exceptions;

public class InsufficientStockException : InvalidOperationException
{
    public InsufficientStockException(string message) : base(message) { }
}