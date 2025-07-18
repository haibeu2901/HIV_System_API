# Stripe Payment Integration Setup Guide

## Overview
This integration allows patients to pay for ARV regimens using Stripe sandbox environment. The payment flow integrates with your existing API endpoint: `https://localhost:7009/api/Payment/CreatePayment`

## Setup Steps

### 1. Stripe Account Setup
1. Go to [Stripe Dashboard](https://dashboard.stripe.com/register)
2. Create a Stripe account or log in
3. Enable **Test Mode** (toggle in top-right)
4. Go to **Developers** > **API Keys**
5. Copy your **Publishable Key** (starts with `pk_test_`)

### 2. Update Configuration
Update the Stripe public key in `payment.js`:
```javascript
const PAYMENT_CONFIG = {
    API_BASE_URL: 'https://localhost:7009/api',
    STRIPE_PUBLIC_KEY: 'pk_test_your_actual_stripe_public_key_here', // Replace this
    SUPPORTED_CURRENCIES: ['USD', 'VND'],
    PAYMENT_METHODS: ['card', 'stripe']
};
```

### 3. Backend API Integration
Your API endpoint should handle the payment request and return a response. Example expected flow:

**Request to your API:**
```json
{
  "pmrId": 123,
  "srvId": 1,
  "amount": 500000,
  "currency": "VND",
  "paymentMethod": "stripe",
  "description": "ARV Regimen Payment - ID: 123"
}
```

**Expected Response:**
```json
{
  "paymentId": "pay_123456789",
  "status": "succeeded",
  "amount": 500000,
  "currency": "VND",
  "clientSecret": "pi_123456789_secret_123" // If using Payment Intents
}
```

### 4. Test Credit Cards
Use these test credit cards in Stripe sandbox:

- **Success**: `4242424242424242` (Visa)
- **Declined**: `4000000000000002` (Visa)
- **Insufficient Funds**: `4000000000009995` (Visa)
- **Expired Card**: `4000000000000069` (Visa)

**Test Details:**
- Any future expiry date (e.g., 12/25)
- Any 3-digit CVC (e.g., 123)
- Any valid ZIP code (e.g., 12345)

### 5. Currency Configuration
The system supports VND (Vietnamese Dong) and USD. Stripe minimum amounts:
- **VND**: 23,000 VND (≈ $1 USD)
- **USD**: $0.50 USD

### 6. Security Configuration
1. **HTTPS Required**: Stripe requires HTTPS in production
2. **API Authentication**: Your API should validate the Bearer token
3. **Webhook Verification**: Implement webhook endpoints for payment confirmation

## Files Structure
```
private-view/user-view/payment/
├── payment.js          # Payment service and modal logic
├── payment.css         # Payment modal styles
├── payment-test.html   # Test page for payment integration
```

## Integration Points

### Medical Record Integration
- Payment buttons appear on ARV regimens with `totalCost > 0`
- Payment is triggered when user clicks "Pay Now"
- Payment data includes regimen ID, amount, and description

### Payment Flow
1. User clicks "Pay Now" on ARV regimen
2. Payment modal opens with Stripe Elements
3. User enters payment details
4. Payment is processed through your API
5. Success/failure feedback is shown

## API Endpoint Details

### POST /api/Payment/CreatePayment
**Headers:**
```
Content-Type: application/json
Authorization: Bearer {token}
```

**Body:**
```json
{
  "pmrId": 0,        // Patient medical record ID or regimen ID
  "srvId": 0,        // Service ID (1 for ARV regimen)
  "amount": 0,       // Amount in smallest currency unit
  "currency": "string",  // "VND" or "USD"
  "paymentMethod": "string",  // "stripe"
  "description": "string"     // Payment description
}
```

## Testing
1. Open `payment-test.html` in your browser
2. Click "Pay Now" to test the payment flow
3. Use test credit card numbers
4. Check your Stripe dashboard for payment logs

## Production Deployment
1. Replace `STRIPE_PUBLIC_KEY` with production key
2. Update API endpoints from localhost to production
3. Ensure HTTPS is enabled
4. Configure webhook endpoints
5. Test with real payment amounts

## Troubleshooting

### Common Issues
1. **"Stripe not initialized"**: Check if Stripe public key is valid
2. **CORS errors**: Ensure your API allows cross-origin requests
3. **Payment fails**: Check API authentication and request format
4. **Modal not showing**: Verify payment scripts are loaded

### Debug Steps
1. Open browser DevTools (F12)
2. Check Console for JavaScript errors
3. Check Network tab for API call responses
4. Verify Stripe Dashboard for payment attempts

## Support
- **Stripe Documentation**: https://stripe.com/docs
- **Stripe Test Cards**: https://stripe.com/docs/testing
- **Webhook Testing**: https://stripe.com/docs/webhooks

## Security Notes
- Never expose secret keys in frontend code
- Always validate payments on your backend
- Use HTTPS in production
- Implement proper error handling
- Log payment attempts for debugging
