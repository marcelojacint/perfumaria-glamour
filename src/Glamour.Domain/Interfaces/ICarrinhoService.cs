using Glamour.Domain.Entities;

namespace Glamour.Domain.Interfaces;

public record ItemCarrinho(Guid ProdutoId, string Nome, string? UrlImagem, decimal Preco, int Quantidade);

public interface ICarrinhoService
{
    Task<List<ItemCarrinho>> ObterCarrinhoAsync(string carrinhoId);
    Task AdicionarItemAsync(string carrinhoId, ItemCarrinho item);
    Task RemoverItemAsync(string carrinhoId, Guid produtoId);
    Task AtualizarQuantidadeAsync(string carrinhoId, Guid produtoId, int quantidade);
    Task LimparCarrinhoAsync(string carrinhoId);
    Task<int> ObterQuantidadeItensAsync(string carrinhoId);
}
