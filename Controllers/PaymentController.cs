using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using BlazorApp1.Models;
using Npgsql;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BlazorApp1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly StripeSettings _stripeSettings;
    private readonly ILogger<PaymentController> _logger;
    private readonly NpgsqlDataSource _dataSource;

    public PaymentController(IOptions<StripeSettings> stripeSettings, ILogger<PaymentController> logger, NpgsqlDataSource dataSource)
    {
        _stripeSettings = stripeSettings.Value;
        _logger = logger;
        _dataSource = dataSource;
    }

    [HttpPost("create-checkout-session")]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
    {
        try
        {
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = request.ProductName,
                            },
                            UnitAmount = request.Amount,
                        },
                        Quantity = request.Quantity,
                    },
                },
                Mode = "payment",
                // Redirect to Angular app after successful payment
                SuccessUrl = $"{_stripeSettings.AngularAppUrl}/payment-success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{_stripeSettings.AngularAppUrl}/credit-shop",
                // Store hearts and user ID in metadata for webhook processing
                Metadata = new Dictionary<string, string>
                {
                    { "hearts", request.Hearts.ToString() },
                    { "user_id", request.UserId }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return Ok(new { sessionId = session.Id, url = session.Url });
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error occurred");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating checkout session");
            return StatusCode(500, new { error = "An error occurred while creating the checkout session" });
        }
    }

    [HttpGet("session/{sessionId}")]
    [AllowAnonymous] // Allow access without authentication
    public async Task<IActionResult> GetSession(string sessionId)
    {
        try
        {
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            var service = new SessionService();
            var session = await service.GetAsync(sessionId);

            // Extract metadata
            int? hearts = null;
            int? userId = null;
            int? currentHearts = null;

            if (session.Metadata != null)
            {
                if (session.Metadata.TryGetValue("hearts", out var heartsStr) && int.TryParse(heartsStr, out var heartsValue))
                {
                    hearts = heartsValue;
                }

                if (session.Metadata.TryGetValue("user_id", out var userIdStr) && int.TryParse(userIdStr, out var userIdValue))
                {
                    userId = userIdValue;

                    // If payment is complete, get the current hearts balance from the database
                    if (session.PaymentStatus == "paid" && userId.HasValue)
                    {
                        try
                        {
                            await using var connection = await _dataSource.OpenConnectionAsync();
                            await using var command = new NpgsqlCommand(
                                "SELECT hearts FROM users WHERE id = @userId",
                                connection);
                            command.Parameters.AddWithValue("userId", userId.Value);

                            var result = await command.ExecuteScalarAsync();
                            if (result != null)
                            {
                                currentHearts = Convert.ToInt32(result);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not fetch current hearts for user {UserId}", userId);
                        }
                    }
                }
            }

            return Ok(new
            {
                status = session.PaymentStatus,
                customerEmail = session.CustomerEmail,
                amountTotal = session.AmountTotal,
                hearts = hearts,
                userId = userId,
                currentHearts = currentHearts,
                isPaid = session.PaymentStatus == "paid"
            });
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error occurred");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _stripeSettings.WebhookSecret
            );

            // Handle the event
            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                _logger.LogInformation("Payment successful for session: {SessionId}", session?.Id);

                await ProcessSuccessfulPayment(session);
            }
            else if (stripeEvent.Type == "checkout.session.async_payment_succeeded")
            {
                var session = stripeEvent.Data.Object as Session;
                _logger.LogInformation("Async payment successful for session: {SessionId}", session?.Id);

                await ProcessSuccessfulPayment(session);
            }
            else if (stripeEvent.Type == "checkout.session.async_payment_failed")
            {
                var session = stripeEvent.Data.Object as Session;
                _logger.LogWarning("Async payment failed for session: {SessionId}", session?.Id);
            }
            else
            {
                _logger.LogInformation("Unhandled event type: {EventType}", stripeEvent.Type);
            }

            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Webhook signature verification failed");
            return BadRequest();
        }
    }

    private async Task ProcessSuccessfulPayment(Session? session)
    {
        if (session?.Metadata == null)
        {
            _logger.LogWarning("Session metadata is null for session: {SessionId}", session?.Id);
            return;
        }

        try
        {
            // Extract metadata
            if (!session.Metadata.TryGetValue("hearts", out var heartsStr) ||
                !session.Metadata.TryGetValue("user_id", out var userIdStr))
            {
                _logger.LogWarning("Missing metadata (hearts or user_id) for session: {SessionId}", session.Id);
                return;
            }

            if (!int.TryParse(heartsStr, out var hearts) || !int.TryParse(userIdStr, out var userId))
            {
                _logger.LogWarning("Invalid metadata format for session: {SessionId}", session.Id);
                return;
            }

            // Update user hearts in database
            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(
                @"UPDATE users
                  SET hearts = hearts + @hearts, updated_at = @updatedAt
                  WHERE id = @userId
                  RETURNING hearts",
                connection);

            command.Parameters.AddWithValue("hearts", hearts);
            command.Parameters.AddWithValue("userId", userId);
            command.Parameters.AddWithValue("updatedAt", DateTime.UtcNow);

            var newHearts = await command.ExecuteScalarAsync();

            _logger.LogInformation(
                "Successfully added {Hearts} hearts to user {UserId}. New balance: {NewHearts}",
                hearts, userId, newHearts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for session: {SessionId}", session.Id);
        }
    }
}
