using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using BlazorApp1.Models;

namespace BlazorApp1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly StripeSettings _stripeSettings;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IOptions<StripeSettings> stripeSettings, ILogger<PaymentController> logger)
    {
        _stripeSettings = stripeSettings.Value;
        _logger = logger;
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
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/payment-success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/payment-cancel",
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
    public async Task<IActionResult> GetSession(string sessionId)
    {
        try
        {
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            var service = new SessionService();
            var session = await service.GetAsync(sessionId);

            return Ok(new
            {
                status = session.PaymentStatus,
                customerEmail = session.CustomerEmail,
                amountTotal = session.AmountTotal
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

                // Here you can:
                // 1. Update your database with the payment status
                // 2. Send confirmation email
                // 3. Fulfill the order
                // 4. Grant access to paid content
            }
            else if (stripeEvent.Type == "checkout.session.async_payment_succeeded")
            {
                var session = stripeEvent.Data.Object as Session;
                _logger.LogInformation("Async payment successful for session: {SessionId}", session?.Id);
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
}
