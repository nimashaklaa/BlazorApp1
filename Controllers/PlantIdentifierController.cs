using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Security.Claims;

namespace BlazorApp1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlantIdentifierController : ControllerBase
{
    private readonly NpgsqlDataSource _dataSource;

    public PlantIdentifierController(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    [HttpPost("identify")]
    public async Task<IActionResult> IdentifyPlant(CancellationToken cancellationToken)
    {
        try
        {
            // Get user ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var userId = int.Parse(userIdClaim.Value);

            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

            // Check current hearts balance
            await using var checkCommand = new NpgsqlCommand(
                "SELECT hearts FROM users WHERE id = @userId",
                connection);
            checkCommand.Parameters.AddWithValue("userId", userId);

            var currentHearts = await checkCommand.ExecuteScalarAsync(cancellationToken);
            if (currentHearts == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var hearts = (int)currentHearts;
            if (hearts < 2)
            {
                return BadRequest(new { message = "Insufficient hearts. You need at least 2 hearts to identify a plant.", currentHearts = hearts });
            }

            // Reduce hearts by 2
            await using var updateCommand = new NpgsqlCommand(
                @"UPDATE users
                  SET hearts = hearts - 2, updated_at = @updatedAt
                  WHERE id = @userId
                  RETURNING hearts",
                connection);
            updateCommand.Parameters.AddWithValue("userId", userId);
            updateCommand.Parameters.AddWithValue("updatedAt", DateTime.UtcNow);

            var newHearts = await updateCommand.ExecuteScalarAsync(cancellationToken);

            return Ok(new
            {
                message = "Plant identification successful",
                heartsUsed = 2,
                remainingHearts = newHearts
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during plant identification", error = ex.Message });
        }
    }

    [HttpGet("hearts")]
    public async Task<IActionResult> GetHearts(CancellationToken cancellationToken)
    {
        try
        {
            // Get user ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var userId = int.Parse(userIdClaim.Value);

            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            await using var command = new NpgsqlCommand(
                "SELECT hearts FROM users WHERE id = @userId",
                connection);
            command.Parameters.AddWithValue("userId", userId);

            var hearts = await command.ExecuteScalarAsync(cancellationToken);
            if (hearts == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new { hearts });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching hearts", error = ex.Message });
        }
    }
}
