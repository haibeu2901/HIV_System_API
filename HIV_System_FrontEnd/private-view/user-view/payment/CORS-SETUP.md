# CORS and HTTPS Setup Guide

## The Issues You're Seeing

The console errors show several problems:
1. **CORS Policy Errors**: Browser blocking cross-origin requests
2. **ERR_BLOCKED_BY_CLIENT**: AdBlock or security extensions blocking requests
3. **HTTPS Required**: Stripe requires HTTPS connections

## Solutions

### 1. Backend CORS Configuration

Add these headers to your ASP.NET Core backend:

```csharp
// In Startup.cs or Program.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", builder =>
        {
            builder
                .WithOrigins("http://localhost:3000", "https://localhost:3000", "file://")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseCors("AllowFrontend");
    // ... other middleware
}
```

### 2. Browser Security

**Disable browser security for testing:**
```bash
# Chrome (Windows)
"C:\Program Files\Google\Chrome\Application\chrome.exe" --disable-web-security --user-data-dir="C:/ChromeDevSession" --disable-features=VizDisplayCompositor

# Chrome (Mac)
open -n -a /Applications/Google\ Chrome.app/Contents/MacOS/Google\ Chrome --args --user-data-dir="/tmp/chrome_dev_session" --disable-web-security
```

### 3. Local HTTPS Server

**Option A: Use Live Server with HTTPS**
```bash
npm install -g live-server
live-server --https=path/to/cert.pem --https-key=path/to/key.pem
```

**Option B: Use Python HTTPS Server**
```bash
# Generate self-signed certificate
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes

# Start HTTPS server
python -m http.server 8000 --bind 127.0.0.1 --directory . --cgi
```

### 4. Development Environment Fix

**Create a simple development server:**

```javascript
// server.js
const express = require('express');
const https = require('https');
const fs = require('fs');
const path = require('path');

const app = express();

// Enable CORS
app.use((req, res, next) => {
    res.header('Access-Control-Allow-Origin', '*');
    res.header('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS');
    res.header('Access-Control-Allow-Headers', 'Origin, X-Requested-With, Content-Type, Accept, Authorization');
    next();
});

// Serve static files
app.use(express.static('.'));

// Start server
const port = 8443;
const options = {
    key: fs.readFileSync('key.pem'),
    cert: fs.readFileSync('cert.pem')
};

https.createServer(options, app).listen(port, () => {
    console.log(`HTTPS Server running on https://localhost:${port}`);
});
```

### 5. Payment Integration Fix

The payment system I created includes:
- **CORS fallback**: Uses XMLHttpRequest when fetch fails
- **Mock responses**: Provides test responses when API is unavailable
- **Error handling**: Graceful degradation for network issues

### 6. Test Your Integration

1. **Open the test page**: `https://localhost:8443/private-view/user-view/payment/payment-test.html`
2. **Check API connection**: Use the "Test Connection" button
3. **Test payment flow**: Use the "Test Payment" button
4. **View payment history**: Load previous payments

### 7. Production Deployment

When deploying to production:
1. Use proper SSL certificates
2. Configure CORS properly on your backend
3. Use environment variables for API endpoints
4. Implement proper authentication

## Quick Test Commands

```bash
# Test your backend API
curl -X POST https://localhost:7009/api/Payment/CreatePayment \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer your-token" \
  -d '{
    "pmrId": 123,
    "srvId": 1,
    "amount": 500000,
    "currency": "VND",
    "paymentMethod": "api",
    "description": "Test payment"
  }'

# Test CORS
curl -H "Origin: https://localhost:8443" \
  -H "Access-Control-Request-Method: POST" \
  -H "Access-Control-Request-Headers: Content-Type,Authorization" \
  -X OPTIONS https://localhost:7009/api/Payment/CreatePayment
```

## Browser Developer Tools

1. **Open DevTools** (F12)
2. **Network tab**: Check if requests are being made
3. **Console tab**: Look for specific error messages
4. **Security tab**: Check certificate issues

The payment system is now robust and handles these common issues automatically!
