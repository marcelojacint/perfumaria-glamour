namespace Glamour.Domain.Interfaces;

public interface IEmailService
{
    Task<bool> EnviarAsync(string assunto, string corpo, CancellationToken cancellationToken = default);
}
