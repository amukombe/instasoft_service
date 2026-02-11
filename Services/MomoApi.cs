using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TransactionNotifier.Models;

namespace TransactionNotifier.Services
{
    class MomoApi
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public MomoApi(IConfiguration config)
        {
            _http = new HttpClient();
            _baseUrl = config["PaymentsApi:BaseUrl"]?? throw new InvalidOperationException("Missing PaymentsApi:BaseUrl");
        }

        public async Task<MomoResponse> GetTransactionStatusAsync(string requestId)
        {
            var url = $"{_baseUrl}/api/Payments/GetTransactionStatus/{requestId}";
            try
            {
                var resp = await _http.GetAsync(url);
                var json = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                    throw new HttpRequestException($"Payments API status call failed: {(int)resp.StatusCode} - {json}");

                return JsonSerializer.Deserialize<MomoResponse>(json,
                                                                 new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        
        }
    }
}
