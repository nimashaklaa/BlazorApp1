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
                    hearts INTEGER NOT NULL DEFAULT 10,
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

    [HttpPost("migrate-hearts")]
    public async Task<IActionResult> MigrateHearts(CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

            // Add hearts column if it doesn't exist
            var migrationSql = @"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name='users' AND column_name='hearts'
                    ) THEN
                        ALTER TABLE users ADD COLUMN hearts INTEGER NOT NULL DEFAULT 10;
                    END IF;
                END $$;
            ";

            await using var command = new NpgsqlCommand(migrationSql, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);

            return Ok(new { message = "Hearts column migration completed successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Migration failed", error = ex.Message });
        }
    }

    [HttpGet("verify-schema")]
    public async Task<IActionResult> VerifySchema(CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

            // Check if hearts column exists
            var checkSql = @"
                SELECT column_name, data_type, column_default
                FROM information_schema.columns
                WHERE table_name = 'users'
                ORDER BY ordinal_position;
            ";

            await using var command = new NpgsqlCommand(checkSql, connection);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var columns = new List<object>();
            while (await reader.ReadAsync(cancellationToken))
            {
                columns.Add(new
                {
                    name = reader.GetString(0),
                    type = reader.GetString(1),
                    defaultValue = reader.IsDBNull(2) ? null : reader.GetString(2)
                });
            }

            return Ok(new { table = "users", columns });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Schema verification failed", error = ex.Message });
        }
    }
}
