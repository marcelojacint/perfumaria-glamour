namespace Glamour.Domain.Interfaces;

public interface IWhatsAppService
{
    Task<bool> EnviarAsync(string mensagem, CancellationToken cancellationToken = default);
}
