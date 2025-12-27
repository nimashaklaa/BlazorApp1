using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Npgsql;
using BlazorApp1.Models;
using BCrypt.Net;

namespace BlazorApp1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly IConfiguration _configuration;

    public AuthController(NpgsqlDataSource dataSource, IConfiguration configuration)
    {
        _dataSource = dataSource;
        _configuration = configuration;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if user already exists
            await using var checkConnection = await _dataSource.OpenConnectionAsync(cancellationToken);
            await using var checkCommand = new NpgsqlCommand(
                "SELECT COUNT(*) FROM users WHERE email = @email",
                checkConnection);
            checkCommand.Parameters.AddWithValue("email", request.Email.ToLower());

            var exists = (long)(await checkCommand.ExecuteScalarAsync(cancellationToken) ?? 0) > 0;
            if (exists)
            {
                return BadRequest(new { message = "User with this email already exists" });
            }

            // Hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Insert new user
            await using var insertConnection = await _dataSource.OpenConnectionAsync(cancellationToken);
            await using var insertCommand = new NpgsqlCommand(
                @"INSERT INTO users (name, email, password, created_at)
                  VALUES (@name, @email, @password, @created_at)
                  RETURNING id",
                insertConnection);

            insertCommand.Parameters.AddWithValue("name", request.Name);
            insertCommand.Parameters.AddWithValue("email", request.Email.ToLower());
            insertCommand.Parameters.AddWithValue("password", hashedPassword);
            insertCommand.Parameters.AddWithValue("created_at", DateTime.UtcNow);

            var userId = await insertCommand.ExecuteScalarAsync(cancellationToken);

            // Generate JWT token
            var token = GenerateJwtToken(userId?.ToString() ?? "", request.Email, request.Name);

            return Ok(new AuthResponse
            {
                Token = token,
                Email = request.Email,
                Name = request.Name
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during signup", error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Find user by email
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            await using var command = new NpgsqlCommand(
                "SELECT id, name, email, password FROM users WHERE email = @email",
                connection);
            command.Parameters.AddWithValue("email", request.Email.ToLower());

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            var userId = reader.GetInt32(0);
            var name = reader.GetString(1);
            var email = reader.GetString(2);
            var hashedPassword = reader.GetString(3);

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, hashedPassword))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Generate JWT token
            var token = GenerateJwtToken(userId.ToString(), email, name);

            return Ok(new AuthResponse
            {
                Token = token,
                Email = email,
                Name = name
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
        }
    }

    private string GenerateJwtToken(string userId, string email, string name)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "BlazorApp1";
        var audience = jwtSettings["Audience"] ?? "BlazorApp1";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Name, name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
