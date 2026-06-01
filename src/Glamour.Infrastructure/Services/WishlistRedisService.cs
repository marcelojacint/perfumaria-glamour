using System.Text.Json;
using Glamour.Domain.Interfaces;
using StackExchange.Redis;

namespace Glamour.Infrastructure.Services;

public class WishlistRedisService(IConnectionMultiplexer redis) : IWishlistService
{
    private readonly IDatabase _db = redis.GetDatabase();
    private static string Key(string id) => $"wishlist:{id}";

    public async Task<List<Guid>> ObterAsync(string usuarioId)
    {
        var valor = await _db.StringGetAsync(Key(usuarioId));
        if (valor.IsNullOrEmpty) return [];
        return JsonSerializer.Deserialize<List<Guid>>(valor!.ToString()) ?? [];
    }

    public async Task AdicionarAsync(string usuarioId, Guid produtoId)
    {
        var lista = await ObterAsync(usuarioId);
        if (!lista.Contains(produtoId))
        {
            lista.Add(produtoId);
            await _db.StringSetAsync(Key(usuarioId), JsonSerializer.Serialize(lista), TimeSpan.FromDays(90));
        }
    }

    public async Task RemoverAsync(string usuarioId, Guid produtoId)
    {
        var lista = await ObterAsync(usuarioId);
        lista.Remove(produtoId);
        await _db.StringSetAsync(Key(usuarioId), JsonSerializer.Serialize(lista), TimeSpan.FromDays(90));
    }

    public async Task<bool> ContemAsync(string usuarioId, Guid produtoId)
    {
        var lista = await ObterAsync(usuarioId);
        return lista.Contains(produtoId);
    }
}
