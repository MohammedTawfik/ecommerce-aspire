# Keycloak Setup Guide for E-Commerce Basket API

## What I Fixed

1. ✅ **Middleware Order**: Moved `UseAuthentication()` and `UseAuthorization()` BEFORE endpoint mapping
2. ✅ **Added Debug Logging**: Authentication failures will now show in console
3. ✅ **Keycloak Reference**: Added Keycloak reference to Basket project in AppHost
4. ✅ **Created Test Script**: `test-keycloak.ps1` to verify your setup

## Step-by-Step Keycloak Configuration

### 1. Access Keycloak Admin Console

Navigate to: `http://localhost:6001/admin`

**Default Admin Credentials** (check Aspire Dashboard for generated password or look in the logs)

### 2. Create the Realm

1. Click the dropdown in top-left corner (shows "Master")
2. Click **"Create Realm"**
3. **Realm name**: `eshop`
4. Click **"Create"**

### 3. Create the Client

1. Make sure you're in the `eshop` realm
2. Go to **Clients** → **"Create client"**
3. **General Settings**:
   - Client ID: `basket-api`
   - Client Protocol: `openid-connect`
   - Click **"Next"**

4. **Capability Config**:
   - ✅ **Client authentication**: ON (this makes it a confidential client)
   - ✅ **Authorization**: OFF (not needed for this use case)
   - **Authentication flow**:
     - ✅ Standard flow
     - ✅ Direct access grants (IMPORTANT for password grant)
     - ✅ Service accounts roles
   - Click **"Next"**

5. **Login Settings**:
   - Valid redirect URIs: `*` (for development only)
   - Web origins: `*` (for development only)
   - Click **"Save"**

6. **Get Client Secret**:
   - Go to the **"Credentials"** tab
   - Copy the **Client Secret** (you'll need this!)

### 4. Create a Test User

1. Go to **Users** → **"Add user"**
2. **Username**: `testuser`
3. **Email**: (optional) `testuser@example.com`
4. **Email verified**: ON (optional)
5. Click **"Create"**

6. **Set Password**:
   - Go to the **"Credentials"** tab
   - Click **"Set password"**
   - **Password**: `testuser123`
   - **Password confirmation**: `testuser123`
   - **Temporary**: OFF (IMPORTANT!)
   - Click **"Save"**
   - Confirm in the dialog

## Testing Your Setup

### Option 1: Use the PowerShell Test Script

```powershell
.\test-keycloak.ps1
```

This script will:
- Check if Keycloak is accessible
- Verify the realm exists
- Help you get an access token
- Show token claims
- Copy the token to your clipboard

### Option 2: Manual cURL Test

```bash
curl -X POST http://localhost:6001/realms/eshop/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=basket-api" \
  -d "client_secret=YOUR_CLIENT_SECRET_HERE" \
  -d "username=testuser" \
  -d "password=testuser123"
```

### Option 3: PowerShell One-Liner

```powershell
$body = @{
    grant_type = "password"
    client_id = "basket-api"
    client_secret = "YOUR_CLIENT_SECRET"
    username = "testuser"
    password = "testuser123"
}

$response = Invoke-RestMethod -Uri "http://localhost:6001/realms/eshop/protocol/openid-connect/token" -Method Post -Body $body -ContentType "application/x-www-form-urlencoded"

Write-Host "Bearer $($response.access_token)"
```

## Using the Token with Basket API

Once you have the access token, use it in your API requests:

### In Postman
1. Go to **Authorization** tab
2. Type: **Bearer Token**
3. Token: Paste your access token (WITHOUT the "Bearer" prefix)

### In cURL
```bash
curl -X GET http://localhost:YOUR_BASKET_PORT/basket/testuser \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN_HERE"
```

### In PowerShell
```powershell
$headers = @{
    "Authorization" = "Bearer YOUR_ACCESS_TOKEN_HERE"
}

Invoke-RestMethod -Uri "http://localhost:YOUR_BASKET_PORT/basket/testuser" -Headers $headers
```

## Debugging Authentication Issues

### 1. Check Application Logs

After restarting your Basket service, watch the console output. The added logging will show:
- Authentication failures with details
- Token validation success/failure
- Challenge errors

### 2. Common Issues

#### "Invalid username or password"
- ✅ User exists in Keycloak
- ✅ Password is correct
- ✅ Password is not temporary
- ✅ User is enabled

#### "Invalid client credentials"
- ✅ Client secret is correct
- ✅ Client authentication is enabled
- ✅ Client ID matches exactly

#### "Unauthorized" from Basket API
- ✅ Token is valid (not expired)
- ✅ Token is sent with "Bearer " prefix
- ✅ Realm name matches ("eshop")
- ✅ Middleware order is correct (UseAuthentication before MapEndpoints)
- ✅ Basket service references Keycloak in AppHost

#### "Failed to retrieve OIDC configuration"
- ✅ Keycloak is running and accessible
- ✅ Realm exists
- ✅ Network connectivity between services
- ✅ Service discovery is working (check Aspire dashboard)

### 3. Verify Keycloak Configuration Endpoint

Visit: `http://localhost:6001/realms/eshop/.well-known/openid-configuration`

This should return JSON with:
- `issuer`
- `token_endpoint`
- `jwks_uri`
- etc.

If this fails, your realm doesn't exist or Keycloak isn't running properly.

## Current Configuration

### AppHost.cs
```csharp
var keyCloack = builder.AddKeycloak("keycloack", 6001)
    .WithDataVolume("ecommerce-keycloak-data")
    .WithEnvironment("KC_HTTP_ENABLED", "true")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.e_commerce_Basket>("e-commerce-basket")
    .WithReference(redis)
    .WithReference(catalog)
    .WithReference(rabbitMq)
    .WithReference(keyCloack)  // ← Keycloak reference added
    .WaitFor(redis)
    .WaitFor(rabbitMq)
    .WaitFor(keyCloack);
```

### Basket Program.cs Authentication
- Service name: `keycloack`
- Realm: `eshop`
- HTTPS validation: Disabled (development)
- Audience validation: Disabled (development)
- Debug logging: Enabled

## Next Steps After Getting Token

1. **Restart your Aspire application** to apply the middleware fix
2. **Run the test script** to verify Keycloak setup
3. **Get a token** using one of the methods above
4. **Test a Basket endpoint** with the token
5. **Check the logs** in the Basket service console for authentication debug messages

## Security Notes

⚠️ **For Development Only**:
- Wildcard redirect URIs
- HTTPS validation disabled
- Audience validation disabled
- Simple passwords

🔒 **For Production**:
- Enable HTTPS validation
- Enable audience validation
- Set specific redirect URIs
- Use strong passwords
- Store client secrets securely
- Use proper user management

