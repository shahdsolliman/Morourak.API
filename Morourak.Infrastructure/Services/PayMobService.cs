using Microsoft.Extensions.Options;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Infrastructure.Settings;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Morourak.Infrastructure.Services;

public class PayMobService : IPayMobService
{
    private readonly HttpClient _httpClient;
    private readonly PayMobSettings _settings;

    public PayMobService(HttpClient httpClient, IOptions<PayMobSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
    }

    public async Task<string> GetPaymentTokenAsync(ServiceRequest request, decimal amount)
    {
        // Step 1: Authentication Request
        var authResponse = await _httpClient.PostAsJsonAsync("auth/tokens", new { api_key = _settings.ApiKey });
        authResponse.EnsureSuccessStatusCode();
        var authResult = await authResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = authResult.GetProperty("token").GetString();

        // Step 2: Order Registration Request
        var orderResponse = await _httpClient.PostAsJsonAsync("ecommerce/orders", new
        {
            auth_token = token,
            delivery_needed = "false",
            amount_cents = (int)(amount * 100),
            currency = "EGP",
            items = new[]
            {
                new { name = request.ServiceType.ToString(), amount_cents = (int)(amount * 100), description = request.RequestNumber }
            }
        });
        orderResponse.EnsureSuccessStatusCode();
        var orderResult = await orderResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orderId = orderResult.GetProperty("id").GetInt32();

        // Step 3: Payment Key Request
        var paymentKeyResponse = await _httpClient.PostAsJsonAsync("acceptance/payment_keys", new
        {
            auth_token = token,
            amount_cents = (int)(amount * 100),
            expiration = 3600,
            order_id = orderId,
            billing_data = new
            {
                apartment = "NA",
                email = "citizen@morourak.gov.eg", // Placeholder or get from citizen
                floor = "NA",
                first_name = "Morourak",
                street = "NA",
                building = "NA",
                phone_number = "0123456789", // Placeholder
                shipping_method = "NA",
                postal_code = "NA",
                city = "Cairo",
                country = "EG",
                last_name = "User",
                state = "Cairo"
            },
            currency = "EGP",
            integration_id = int.Parse(_settings.IntegrationId),
            lock_order_when_paid = "false"
        });
        paymentKeyResponse.EnsureSuccessStatusCode();
        var paymentKeyResult = await paymentKeyResponse.Content.ReadFromJsonAsync<JsonElement>();
        return paymentKeyResult.GetProperty("token").GetString() ?? throw new Exception("Failed to get payment token");
    }

    public Task<bool> ValidateWebhookSignatureAsync(IDictionary<string, string> payload, string hmac)
    {
        // PayMob HMAC calculation for version 4 (Standard)
        // Fields in order: amount_cents, created_at, currency, error_occured, has_parent_transaction, id, integration_id, is_3d_secure, is_auth, is_capture, is_refunded, is_standalone_payment, source_data.pan, source_data.sub_type, source_data.type, success
        
        var keys = new[] { "amount_cents", "created_at", "currency", "error_occured", "has_parent_transaction", "id", "integration_id", "is_3d_secure", "is_auth", "is_capture", "is_refunded", "is_standalone_payment", "source_data.pan", "source_data.sub_type", "source_data.type", "success" };
        
        var stringBuilder = new StringBuilder();
        foreach (var key in keys)
        {
            if (payload.TryGetValue(key, out var value))
            {
                stringBuilder.Append(value);
            }
        }

        var calculatedHmac = HmacSha512(_settings.HmacSecret, stringBuilder.ToString());
        return Task.FromResult(calculatedHmac.Equals(hmac, StringComparison.OrdinalIgnoreCase));
    }

    private static string HmacSha512(string key, string input)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}
