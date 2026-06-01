using Glamour.Domain.Interfaces;
using Glamour.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Glamour.Infrastructure.Services;

public class FidelidadeService(UserManager<ApplicationUser> userManager) : IFidelidadeService
{
    // 1 ponto por R$ 1 gasto; 100 pontos = R$ 1 de desconto
    public async Task CreditarPontosAsync(string usuarioId, decimal valorPedido)
    {
        var usuario = await userManager.FindByIdAsync(usuarioId);
        if (usuario == null) return;
        usuario.PontosLoyalty += (int)Math.Floor(valorPedido);
        await userManager.UpdateAsync(usuario);
    }

    public async Task<int> ObterPontosAsync(string usuarioId)
    {
        var usuario = await userManager.FindByIdAsync(usuarioId);
        return usuario?.PontosLoyalty ?? 0;
    }

    public async Task<bool> RegatarPontosAsync(string usuarioId, int pontos)
    {
        var usuario = await userManager.FindByIdAsync(usuarioId);
        if (usuario == null || usuario.PontosLoyalty < pontos) return false;
        usuario.PontosLoyalty -= pontos;
        await userManager.UpdateAsync(usuario);
        return true;
    }
}
