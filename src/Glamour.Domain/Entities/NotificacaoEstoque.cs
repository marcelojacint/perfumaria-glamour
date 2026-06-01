namespace Glamour.Domain.Entities;

public class NotificacaoEstoque : BaseEntity
{
    public Guid ProdutoId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public bool Notificado { get; private set; }
    public DateTime? NotificadoEm { get; private set; }

    protected NotificacaoEstoque() { }

    public NotificacaoEstoque(Guid produtoId, string email)
    {
        ProdutoId = produtoId;
        Email = email.ToLowerInvariant();
    }

    public void MarcarNotificado()
    {
        Notificado = true;
        NotificadoEm = DateTime.UtcNow;
        MarcarAtualizado();
    }
}
