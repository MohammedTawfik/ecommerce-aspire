# Keycloak Testing Script
# This script helps you test your Keycloak configuration

Write-Host "=== Keycloak Configuration Test ===" -ForegroundColor Cyan
Write-Host ""

# Configuration
$keycloakUrl = "http://localhost:6001"
$realm = "eshop"
$clientId = "basket-api"
$clientSecret = Read-Host "Enter your client secret from Keycloak (leave empty to skip)"
$username = "testuser"
$password = "testuser123"

Write-Host ""
Write-Host "Step 1: Testing Keycloak availability..." -ForegroundColor Yellow

try {
    $response = Invoke-WebRequest -Uri "$keycloakUrl/realms/$realm" -Method Get -UseBasicParsing
    Write-Host "✓ Keycloak is running and realm '$realm' exists" -ForegroundColor Green
    
    $realmInfo = $response.Content | ConvertFrom-Json
    Write-Host "  Issuer: $($realmInfo.issuer)" -ForegroundColor Gray
    Write-Host "  Token Endpoint: $($realmInfo.token_endpoint)" -ForegroundColor Gray
}
catch {
    Write-Host "✗ Failed to connect to Keycloak or realm doesn't exist" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Make sure:" -ForegroundColor Yellow
    Write-Host "  1. Keycloak is running on port 6001" -ForegroundColor Yellow
    Write-Host "  2. Realm 'eshop' exists in Keycloak" -ForegroundColor Yellow
    exit
}

Write-Host ""
Write-Host "Step 2: Testing token acquisition..." -ForegroundColor Yellow

if ([string]::IsNullOrWhiteSpace($clientSecret)) {
    Write-Host "⚠ Skipping token test (no client secret provided)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To get a token manually:" -ForegroundColor Cyan
    Write-Host @"
POST $keycloakUrl/realms/$realm/protocol/openid-connect/token
Content-Type: application/x-www-form-urlencoded

Body:
grant_type=password
client_id=$clientId
client_secret=YOUR_CLIENT_SECRET
username=$username
password=$password
"@ -ForegroundColor Gray
}
else {
    try {
        $tokenBody = @{
            grant_type = "password"
            client_id = $clientId
            client_secret = $clientSecret
            username = $username
            password = $password
        }
        
        $tokenResponse = Invoke-RestMethod -Uri "$keycloakUrl/realms/$realm/protocol/openid-connect/token" -Method Post -Body $tokenBody -ContentType "application/x-www-form-urlencoded"
        
        Write-Host "✓ Token acquired successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Access Token (first 50 chars):" -ForegroundColor Cyan
        Write-Host $tokenResponse.access_token.Substring(0, [Math]::Min(50, $tokenResponse.access_token.Length)) -ForegroundColor Gray
        Write-Host ""
        Write-Host "Token Type: $($tokenResponse.token_type)" -ForegroundColor Gray
        Write-Host "Expires In: $($tokenResponse.expires_in) seconds" -ForegroundColor Gray
        Write-Host ""
        
        # Decode JWT to see claims
        $tokenParts = $tokenResponse.access_token.Split('.')
        if ($tokenParts.Length -ge 2) {
            $payload = $tokenParts[1]
            # Add padding if needed
            while ($payload.Length % 4 -ne 0) {
                $payload += "="
            }
            $payloadJson = [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($payload))
            $claims = $payloadJson | ConvertFrom-Json
            
            Write-Host "Token Claims:" -ForegroundColor Cyan
            Write-Host "  Issuer: $($claims.iss)" -ForegroundColor Gray
            Write-Host "  Subject: $($claims.sub)" -ForegroundColor Gray
            Write-Host "  Audience: $($claims.aud)" -ForegroundColor Gray
            Write-Host "  Expiration: $([DateTimeOffset]::FromUnixTimeSeconds($claims.exp).LocalDateTime)" -ForegroundColor Gray
            Write-Host "  Issued At: $([DateTimeOffset]::FromUnixTimeSeconds($claims.iat).LocalDateTime)" -ForegroundColor Gray
        }
        
        Write-Host ""
        Write-Host "Full Bearer Token (copy this):" -ForegroundColor Cyan
        Write-Host "Bearer $($tokenResponse.access_token)" -ForegroundColor White
        
        # Copy to clipboard if possible
        try {
            Set-Clipboard -Value "Bearer $($tokenResponse.access_token)"
            Write-Host ""
            Write-Host "✓ Token copied to clipboard!" -ForegroundColor Green
        }
        catch {
            # Clipboard not available
        }
    }
    catch {
        Write-Host "✗ Failed to get token" -ForegroundColor Red
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        
        if ($_.ErrorDetails.Message) {
            try {
                $errorJson = $_.ErrorDetails.Message | ConvertFrom-Json
                Write-Host "  Keycloak Error: $($errorJson.error_description)" -ForegroundColor Red
            }
            catch {
                Write-Host "  Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
            }
        }
        
        Write-Host ""
        Write-Host "Common issues:" -ForegroundColor Yellow
        Write-Host "  1. Wrong client secret" -ForegroundColor Yellow
        Write-Host "  2. User doesn't exist or wrong password" -ForegroundColor Yellow
        Write-Host "  3. Client doesn't have 'Direct access grants' enabled" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=== Keycloak Setup Checklist ===" -ForegroundColor Cyan
Write-Host "□ Keycloak is running on http://localhost:6001" -ForegroundColor White
Write-Host "□ Realm 'eshop' is created" -ForegroundColor White
Write-Host "□ Client 'basket-api' is created with:" -ForegroundColor White
Write-Host "    - Client authentication: ON" -ForegroundColor Gray
Write-Host "    - Direct access grants: ENABLED" -ForegroundColor Gray
Write-Host "    - Service accounts roles: ENABLED" -ForegroundColor Gray
Write-Host "□ User 'testuser' exists with password 'testuser123'" -ForegroundColor White
Write-Host "□ Password is set as permanent (not temporary)" -ForegroundColor White
Write-Host ""

