using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace BlazorApp1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly NpgsqlDataSource _dataSource;

    public DatabaseController(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    [HttpGet("health")]
    public async Task<IActionResult> Health(CancellationToken cancellationToken)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand("SELECT 1", connection);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Ok(new { status = "ok", result });
    }

    [HttpPost("setup")]
    public async Task<IActionResult> Setup(CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

            // Create users table
            var createTableSql = @"
                CREATE TABLE IF NOT EXISTS users (
                    id SERIAL PRIMARY KEY,
                    name VARCHAR(255) NOT NULL,
                    email VARCHAR(255) NOT NULL UNIQUE,
                    password VARCHAR(255) NOT NULL,
                    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP NULL
                );

                CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
            ";

            await using var command = new NpgsqlCommand(createTableSql, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);

            return Ok(new { message = "Database setup completed successfully", tablesCreated = new[] { "users" } });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Database setup failed", error = ex.Message });
        }
    }
}
