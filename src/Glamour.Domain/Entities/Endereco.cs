namespace Glamour.Domain.Entities;

public class Endereco : BaseEntity
{
    public string UsuarioId { get; private set; } = string.Empty;
    public string CEP { get; private set; } = string.Empty;
    public string Logradouro { get; private set; } = string.Empty;
    public string Numero { get; private set; } = string.Empty;
    public string? Complemento { get; private set; }
    public string Bairro { get; private set; } = string.Empty;
    public string Cidade { get; private set; } = string.Empty;
    public string UF { get; private set; } = string.Empty;
    public string? Apelido { get; private set; }
    public bool Principal { get; private set; }

    protected Endereco() { }

    public Endereco(string usuarioId, string cep, string logradouro, string numero,
        string? complemento, string bairro, string cidade, string uf, string? apelido = null)
    {
        UsuarioId = usuarioId;
        CEP = cep;
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        Cidade = cidade;
        UF = uf.ToUpper();
        Apelido = apelido;
    }

    public void DefinirComoPrincipal() { Principal = true; MarcarAtualizado(); }
    public void RemoverPrincipal() { Principal = false; MarcarAtualizado(); }
}
