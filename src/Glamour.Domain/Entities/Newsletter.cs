namespace Glamour.Domain.Entities;

public class Newsletter : BaseEntity
{
    public string Email { get; private set; } = string.Empty;
    public bool Ativo { get; private set; } = true;

    protected Newsletter() { }

    public Newsletter(string email)
    {
        Email = email.ToLowerInvariant().Trim();
    }

    public void Cancelar() { Ativo = false; MarcarAtualizado(); }
}
