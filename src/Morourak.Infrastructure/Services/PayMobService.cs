using Microsoft.Extensions.Options;
using Morourak.Application.DTOs.Paymob;
using Morourak.Application.Interfaces.Services;
using Morourak.Infrastructure.Settings;
using System.Net.Http.Json;
using System.Text.Json;

namespace Morourak.Infrastructure.Services;

/// <summary>
/// Service for integrating with Paymob Payment Gateway.
/// This implementation uses the 3-step auth/order/payment_key flow.
/// </summary>
public class PayMobService : IPayMobService
{
    private readonly HttpClient _httpClient;
    private readonly PayMobSettings _settings;

    public PayMobService(HttpClient _httpClient, IOptions<PayMobSettings> settings)
    {
        this._httpClient = _httpClient;
        _settings = settings.Value;
        
        if (string.IsNullOrEmpty(this._httpClient.BaseAddress?.ToString()))
        {
            this._httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        }
    }

    public async Task<PaymobPaymentResponse> InitiatePaymentAsync(
        decimal amount,
        string merchantOrderId,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string country,
        string city,
        string street,
        string building)
    {
        try
        {
            // Step 1: Authentication
            var token = await GetAuthTokenAsync();

            // Step 2: Order Registration
            var orderId = await CreateOrderAsync(token, amount, merchantOrderId);

            // Step 3: Payment Key Request
            var paymentKeyToken = await GeneratePaymentKeyAsync(token, amount, orderId, firstName, lastName, email, phoneNumber, country, city, street, building);

            // Construct the payment URL for WebView integration
            var paymentUrl = $"https://accept.paymob.com/api/acceptance/iframes/{_settings.IframeId}?payment_token={paymentKeyToken}";

            return new PaymobPaymentResponse
            {
                PaymentToken = paymentKeyToken,
                PaymobOrderId = orderId.ToString(),
                PaymentUrl = paymentUrl,
                MerchantOrderId = merchantOrderId,
                
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Paymob Initiation Failed: {ex.Message}", ex);
        }
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var response = await _httpClient.PostAsJsonAsync("auth/tokens", new
        {
            api_key = _settings.ApiKey
        });

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Exception($"Authentication failed: {err}");
        }

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();

        return result.GetProperty("token").GetString()
               ?? throw new Exception("Auth token not found in response.");
    }

    private async Task<int> CreateOrderAsync(string token, decimal amount, string merchantOrderId)
    {
        var payload = new
        {
            auth_token = token,
            delivery_needed = "false",
            amount_cents = (int)Math.Round(amount * 100, MidpointRounding.AwayFromZero),
            currency = "EGP",
            merchant_order_id = merchantOrderId,
            items = Array.Empty<object>()
        };

        var response = await _httpClient.PostAsJsonAsync("ecommerce/orders", payload);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Exception($"Order creation failed: {err}");
        }
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("id").GetInt32();
    }

    private async Task<string> GeneratePaymentKeyAsync(string token, decimal amount, int orderId, string firstName, string lastName, string email, string phoneNumber, string country, string city, string street, string building)
    {
        var payload = new
        {
            auth_token = token,
            amount_cents = (int)Math.Round(amount * 100, MidpointRounding.AwayFromZero),
            expiration = 3600,
            order_id = orderId,
            billing_data = new
            {
                first_name = firstName,
                last_name = lastName,
                email = email,
                phone_number = phoneNumber,
                country = country ?? "Egypt",
                city = city ?? "Cairo",
                street = street ?? "NA",
                building = building ?? "NA",
                floor = "NA",
                apartment = "NA",
                shipping_method = "NA",
                postal_code = "NA",
                state = "NA"
            },
            currency = "EGP",
            integration_id = int.Parse(_settings.IntegrationId),
            lock_order_when_paid = "true"
        };

        var response = await _httpClient.PostAsJsonAsync("acceptance/payment_keys", payload);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Exception($"Payment key generation failed: {err}");
        }
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("token").GetString() ?? throw new Exception("Payment key token missing.");
    }

    public bool ValidateWebhookSignature(string hmacHeader, string requestBody)
    {
        // Paymob HMAC calculation: Concatenate specific field values in order and hash with SecretKey.
        // Key fields must be visited in this exact order (per Paymob docs).

        using var jsonDoc = JsonDocument.Parse(requestBody);
        var obj = jsonDoc.RootElement.GetProperty("obj");

        // Key fields for HMAC calculation (exact order from docs)
        string[] keys = {
            "amount_cents", "created_at", "currency", "error_occured",
            "has_parent_transaction", "id", "integration_id", "is_3d_secure",
            "is_auth", "is_capture", "is_refunded", "is_standalone_payment",
            "is_voided", "order", "owner", "pending", "source_data.pan",
            "source_data.sub_type", "source_data.type", "success"
        };

        var sb = new System.Text.StringBuilder();
        foreach (var key in keys)
        {
            string val = "";
            if (key == "source_data.pan")
                val = obj.TryGetProperty("source_data", out var sd1) && sd1.TryGetProperty("pan", out var pan) ? pan.ToString() : "";
            else if (key == "source_data.sub_type")
                val = obj.TryGetProperty("source_data", out var sd2) && sd2.TryGetProperty("sub_type", out var sub) ? sub.ToString() : "";
            else if (key == "source_data.type")
                val = obj.TryGetProperty("source_data", out var sd3) && sd3.TryGetProperty("type", out var typ) ? typ.ToString() : "";
            else if (key == "order")
                val = obj.TryGetProperty("order", out var ord) && ord.TryGetProperty("id", out var oid) ? oid.ToString() : "";
            else if (obj.TryGetProperty(key, out var prop))
            {
                // Paymob expects booleans as lowercase "true"/"false"
                val = prop.ValueKind switch
                {
                    JsonValueKind.True  => "true",
                    JsonValueKind.False => "false",
                    JsonValueKind.Null  => "",
                    _                   => prop.ToString()
                };
            }
            sb.Append(val);
        }

        // ── SECURITY FIX: Constant-time comparison ────────────────────────────
        // string.Equals short-circuits on first differing character, allowing
        // attackers to measure response time and brute-force the HMAC byte-by-byte.
        // CryptographicOperations.FixedTimeEquals always compares all bytes,
        // making timing attacks computationally infeasible.
        // ─────────────────────────────────────────────────────────────────────
        var computedBytes  = System.Text.Encoding.UTF8.GetBytes(CalculateHmac(sb.ToString(), _settings.HmacSecret));
        var receivedBytes  = System.Text.Encoding.UTF8.GetBytes(hmacHeader);

        return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
            computedBytes, receivedBytes);
    }

    private string CalculateHmac(string message, string secret)
    {
        var encoding = new System.Text.UTF8Encoding();
        byte[] keyBytes = encoding.GetBytes(secret);
        byte[] messageBytes = encoding.GetBytes(message);
        using var hmac = new System.Security.Cryptography.HMACSHA256(keyBytes);
        byte[] hashBytes = hmac.ComputeHash(messageBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower(); // Final hash is usually lowercase hex
    }
}
