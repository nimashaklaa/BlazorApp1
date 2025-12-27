namespace BlazorApp1.Models;

public class AuthResponse
{
    public required string Token { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public int Hearts { get; set; }
    public string UserId { get; set; } = string.Empty;
}
