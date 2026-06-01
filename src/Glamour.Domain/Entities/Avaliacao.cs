namespace Glamour.Domain.Entities;

public class Avaliacao : BaseEntity
{
    public Guid ProdutoId { get; private set; }
    public Produto Produto { get; private set; } = null!;
    public string UsuarioId { get; private set; } = string.Empty;
    public string NomeUsuario { get; private set; } = string.Empty;
    public int Nota { get; private set; }
    public string? Comentario { get; private set; }
    public bool Aprovada { get; private set; }

    protected Avaliacao() { }

    public Avaliacao(Guid produtoId, string usuarioId, string nomeUsuario, int nota, string? comentario)
    {
        if (nota < 1 || nota > 5) throw new ArgumentOutOfRangeException(nameof(nota), "Nota deve ser entre 1 e 5.");
        ProdutoId = produtoId;
        UsuarioId = usuarioId;
        NomeUsuario = nomeUsuario;
        Nota = nota;
        Comentario = comentario;
    }

    public void Aprovar() { Aprovada = true; MarcarAtualizado(); }
}
