using System.Net.Http.Headers;
using System.Net.Http.Json;
using Glamour.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Glamour.Infrastructure.Services;

public class ResendEmailService(
    HttpClient http,
    IConfiguration config,
    ILogger<ResendEmailService> logger) : IEmailService
{
    public async Task<bool> EnviarAsync(string assunto, string corpo, CancellationToken cancellationToken = default)
    {
        var apiKey = config["Email:ResendApiKey"];
        var de = config["Email:De"];
        var para = config["Email:Para"];
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(de) || string.IsNullOrWhiteSpace(para))
            return false;

        try
        {
            using var requisicao = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
            {
                Content = JsonContent.Create(new
                {
                    from = de,
                    to = new[] { para },
                    subject = assunto,
                    text = corpo
                })
            };
            requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(10));

            var resposta = await http.SendAsync(requisicao, cts.Token);
            if (!resposta.IsSuccessStatusCode)
            {
                var detalhe = await resposta.Content.ReadAsStringAsync(cts.Token);
                logger.LogWarning("Resend retornou {Status}: {Detalhe}", resposta.StatusCode, detalhe);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao enviar e-mail via Resend.");
            return false;
        }
    }
}
