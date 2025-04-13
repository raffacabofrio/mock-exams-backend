﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MockExams.Infra.Sms;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Infra.IA;

public class ChatGptClient : IIAClient
{
    private readonly HttpClient _httpClient;
    private IASettings _settings { get; set; }

    public ChatGptClient(HttpClient httpClient, IOptions<IASettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<string> GenerateAsync(string prompt)
    {
        if (!_settings.IsActive) throw new IADisabledException("O serviço IA está desativado no appsettings.");

        var requestBody = new
        {
            model = "gpt-4o-mini", // mais barato
            messages = new[]
            {
            new { role = "system", content = "Você é um especialista em simulados técnicos." },
            new { role = "user", content = prompt }
        },
            temperature = 0.7
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
        {
            Content = JsonContent.Create(requestBody)
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return content; // será tratado pelo service depois
    }
}
