using System.Text.Json;
using Glamour.Domain.Interfaces;
using StackExchange.Redis;

namespace Glamour.Infrastructure.Services;

public class CarrinhoRedisService(IConnectionMultiplexer redis) : ICarrinhoService
{
    private readonly IDatabase _db = redis.GetDatabase();
    private static string Key(string id) => $"carrinho:{id}";
    private static readonly TimeSpan Ttl = TimeSpan.FromDays(7);

    public async Task<List<ItemCarrinho>> ObterCarrinhoAsync(string carrinhoId)
    {
        var valor = await _db.StringGetAsync(Key(carrinhoId));
        if (valor.IsNullOrEmpty) return [];
        return JsonSerializer.Deserialize<List<ItemCarrinho>>(valor!.ToString()) ?? [];
    }

    public async Task AdicionarItemAsync(string carrinhoId, ItemCarrinho item)
    {
        var itens = await ObterCarrinhoAsync(carrinhoId);
        var existente = itens.FirstOrDefault(i => i.ProdutoId == item.ProdutoId);

        if (existente != null)
        {
            itens.Remove(existente);
            itens.Add(existente with { Quantidade = existente.Quantidade + item.Quantidade });
        }
        else
        {
            itens.Add(item);
        }

        await SalvarAsync(carrinhoId, itens);
    }

    public async Task RemoverItemAsync(string carrinhoId, Guid produtoId)
    {
        var itens = await ObterCarrinhoAsync(carrinhoId);
        itens.RemoveAll(i => i.ProdutoId == produtoId);
        await SalvarAsync(carrinhoId, itens);
    }

    public async Task AtualizarQuantidadeAsync(string carrinhoId, Guid produtoId, int quantidade)
    {
        var itens = await ObterCarrinhoAsync(carrinhoId);
        var item = itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (item == null) return;
        itens.Remove(item);
        if (quantidade > 0) itens.Add(item with { Quantidade = quantidade });
        await SalvarAsync(carrinhoId, itens);
    }

    public async Task LimparCarrinhoAsync(string carrinhoId) =>
        await _db.KeyDeleteAsync(Key(carrinhoId));

    public async Task<int> ObterQuantidadeItensAsync(string carrinhoId)
    {
        var itens = await ObterCarrinhoAsync(carrinhoId);
        return itens.Sum(i => i.Quantidade);
    }

    private async Task SalvarAsync(string carrinhoId, List<ItemCarrinho> itens) =>
        await _db.StringSetAsync(Key(carrinhoId), JsonSerializer.Serialize(itens), Ttl);
}
