# Stripe Payment Integration Setup Guide

## Overview
Your Blazor application now includes Stripe payment integration with one-time payments using Stripe Checkout.

## What Was Implemented

### 1. Backend Components
- **PaymentController** (`Controllers/PaymentController.cs`)
  - `POST /api/payment/create-checkout-session` - Creates a Stripe checkout session
  - `GET /api/payment/session/{sessionId}` - Retrieves session details
  - `POST /api/payment/webhook` - Handles Stripe webhook events

- **Models**
  - `StripeSettings.cs` - Configuration model for Stripe API keys
  - `CreateCheckoutSessionRequest.cs` - Request model for creating checkout sessions

### 2. Frontend Components
- **Payment Page** (`Components/Pages/Payment.razor`) - Form to initiate payment
- **Success Page** (`Components/Pages/PaymentSuccess.razor`) - Displays after successful payment
- **Cancel Page** (`Components/Pages/PaymentCancel.razor`) - Displays when payment is cancelled

### 3. Configuration
- Stripe settings configured in `Program.cs`
- User secrets set up for API keys

## Setup Instructions

### Step 1: Get Your Stripe API Keys

1. Go to [Stripe Dashboard](https://dashboard.stripe.com)
2. Sign up or log in
3. Navigate to **Developers > API keys**
4. Copy your **Publishable key** and **Secret key** (use test mode keys for development)

### Step 2: Update User Secrets

Replace the placeholder values with your actual Stripe keys:

```bash
dotnet user-secrets set "Stripe:SecretKey" "sk_test_YOUR_ACTUAL_SECRET_KEY"
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_YOUR_ACTUAL_PUBLISHABLE_KEY"
```

### Step 3: Set Up Webhook (Optional for Testing)

For production or testing webhook events:

1. Install Stripe CLI: `brew install stripe/stripe-cli/stripe` (macOS)
2. Login: `stripe login`
3. Forward webhooks to local server:
   ```bash
   stripe listen --forward-to https://localhost:5001/api/payment/webhook
   ```
4. Copy the webhook signing secret and update user secrets:
   ```bash
   dotnet user-secrets set "Stripe:WebhookSecret" "whsec_YOUR_WEBHOOK_SECRET"
   ```

### Step 4: Run the Application

```bash
dotnet run
```

Navigate to `/payment` to test the payment flow.

## Testing Payments

Use Stripe's test card numbers:

- **Success**: `4242 4242 4242 4242`
- **Decline**: `4000 0000 0000 0002`
- **3D Secure**: `4000 0025 0000 3155`

For all test cards:
- Use any future expiration date
- Use any 3-digit CVC
- Use any postal code

## Payment Flow

1. User fills out the payment form on `/payment`
2. Application creates a Stripe checkout session via API
3. User is redirected to Stripe's hosted checkout page
4. User completes payment
5. User is redirected to `/payment-success` or `/payment-cancel`
6. Stripe sends webhook events to `/api/payment/webhook` for payment confirmation

## Webhook Events Handled

- `checkout.session.completed` - Payment succeeded
- `checkout.session.async_payment_succeeded` - Async payment succeeded
- `checkout.session.async_payment_failed` - Async payment failed

## Next Steps

1. **Update webhook handler** in `PaymentController.cs` to:
   - Save payment records to your database
   - Send confirmation emails
   - Fulfill orders
   - Grant access to paid content

2. **Customize the payment form** to match your products/services

3. **Add authentication** to protect payment endpoints if needed

4. **Switch to live mode** when ready:
   - Replace test API keys with live keys
   - Set up production webhooks in Stripe Dashboard

## Important Security Notes

- Never expose your Secret Key in client-side code
- Always validate webhook signatures
- Use HTTPS in production
- Keep your Stripe library updated

## Resources

- [Stripe .NET Documentation](https://stripe.com/docs/api?lang=dotnet)
- [Stripe Checkout Documentation](https://stripe.com/docs/payments/checkout)
- [Stripe Webhooks Guide](https://stripe.com/docs/webhooks)
- [Test Cards](https://stripe.com/docs/testing)
