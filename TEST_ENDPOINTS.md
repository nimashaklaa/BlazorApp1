# Test Your API Endpoints

## Fix for HTTP 411 Error

The 411 error means the server needs a Content-Length header. Here are ways to test:

### Option 1: Use curl with Content-Length (Recommended)

```bash
curl -X POST http://xshop.somee.com/api/database/setup \
  -H "Content-Length: 0" \
  -H "Content-Type: application/json"
```

### Option 2: Use curl with empty JSON body

```bash
curl -X POST http://xshop.somee.com/api/database/setup \
  -H "Content-Type: application/json" \
  -d '{}'
```

### Option 3: Test Health Endpoint (GET - No Content-Length needed)

```bash
curl http://xshop.somee.com/api/database/health
```

This should work without any headers!

### Option 4: Use Browser

Just visit in your browser:
- `http://xshop.somee.com/api/database/health` (GET - should work)
- `http://xshop.somee.com/api/database/setup` (POST - might not work in browser)

### Option 5: Use Postman or Similar Tool

These tools automatically add Content-Length headers.

## Expected Responses

### Health Endpoint (GET)
```json
{"status":"ok","result":1}
```

### Setup Endpoint (POST)
```json
{"message":"Database setup completed successfully","tablesCreated":["users"]}
```

## If Health Endpoint Fails

If `/api/database/health` still gives an error, the issue is likely:
1. Database connection failing
2. Tables don't exist (but health should still work with SELECT 1)
3. Connection string wrong in appsettings.json

Check Somee.com error logs for the exact error!

