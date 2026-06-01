namespace Glamour.Domain.Notifications;

public record Notificacao(string Campo, string Mensagem);

public class NotificacaoContext
{
    private readonly List<Notificacao> _notificacoes = [];
    public IReadOnlyList<Notificacao> Notificacoes => _notificacoes.AsReadOnly();
    public bool Valido => _notificacoes.Count == 0;

    public void Adicionar(string campo, string mensagem) =>
        _notificacoes.Add(new Notificacao(campo, mensagem));

    public void Adicionar(Notificacao notificacao) => _notificacoes.Add(notificacao);
}
