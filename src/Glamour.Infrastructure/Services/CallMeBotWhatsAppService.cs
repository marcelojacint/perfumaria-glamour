using Glamour.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Glamour.Infrastructure.Services;

public class CallMeBotWhatsAppService(
    HttpClient http,
    IConfiguration config,
    ILogger<CallMeBotWhatsAppService> logger) : IWhatsAppService
{
    public async Task<bool> EnviarAsync(string mensagem, CancellationToken cancellationToken = default)
    {
        var numero = config["WhatsApp:Numero"];
        var apiKey = config["WhatsApp:ApiKey"];
        if (string.IsNullOrWhiteSpace(numero) || string.IsNullOrWhiteSpace(apiKey))
            return false;

        try
        {
            var url = $"https://api.callmebot.com/whatsapp.php?phone={Uri.EscapeDataString(numero)}" +
                      $"&text={Uri.EscapeDataString(mensagem)}&apikey={Uri.EscapeDataString(apiKey)}";

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(8));

            var resposta = await http.GetAsync(url, cts.Token);
            return resposta.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao enviar notificação de WhatsApp via CallMeBot.");
            return false;
        }
    }
}
