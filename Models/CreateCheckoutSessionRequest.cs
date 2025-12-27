namespace BlazorApp1.Models;

public class CreateCheckoutSessionRequest
{
    public string ProductName { get; set; } = string.Empty;
    public long Amount { get; set; } // Amount in cents
    public int Quantity { get; set; } = 1;
}
