namespace BlazorApp1.Models;

public class StripeSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string AngularAppUrl { get; set; } = "http://localhost:4200";
}
