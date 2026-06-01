using Glamour.Application.DTOs;
using Glamour.Domain.Entities;
using Glamour.Domain.Interfaces;
using Glamour.Domain.Notifications;

namespace Glamour.Application.Services;

public class AvaliacaoService(IRepository<Avaliacao> repo, NotificacaoContext notificacoes)
{
    public async Task<IEnumerable<AvaliacaoDto>> ObterAprovadosAsync(Guid produtoId)
    {
        var lista = await repo.BuscarAsync(a => a.ProdutoId == produtoId && a.Aprovada);
        return lista.OrderByDescending(a => a.CriadoEm).Select(MapDto);
    }

    public async Task<IEnumerable<AvaliacaoDto>> ObterPendentesAsync()
    {
        var lista = await repo.BuscarAsync(a => !a.Aprovada);
        return lista.OrderByDescending(a => a.CriadoEm).Select(MapDto);
    }

    public async Task<bool> CriarAsync(CriarAvaliacaoDto dto, string usuarioId, string nomeUsuario)
    {
        var jaAvaliou = await repo.BuscarAsync(a => a.ProdutoId == dto.ProdutoId && a.UsuarioId == usuarioId);
        if (jaAvaliou.Any())
        {
            notificacoes.Adicionar("Avaliacao", "Você já avaliou este produto.");
            return false;
        }

        var avaliacao = new Avaliacao(dto.ProdutoId, usuarioId, nomeUsuario, dto.Nota, dto.Comentario);
        await repo.AdicionarAsync(avaliacao);
        await repo.SalvarAsync();
        return true;
    }

    public async Task<bool> AprovarAsync(Guid id)
    {
        var avaliacao = await repo.ObterPorIdAsync(id);
        if (avaliacao == null) return false;
        avaliacao.Aprovar();
        await repo.AtualizarAsync(avaliacao);
        await repo.SalvarAsync();
        return true;
    }

    public async Task<bool> RemoverAsync(Guid id)
    {
        var avaliacao = await repo.ObterPorIdAsync(id);
        if (avaliacao == null) return false;
        await repo.RemoverAsync(avaliacao);
        await repo.SalvarAsync();
        return true;
    }

    private static AvaliacaoDto MapDto(Avaliacao a) =>
        new(a.Id, a.ProdutoId, a.NomeUsuario, a.Nota, a.Comentario, a.Aprovada, a.CriadoEm);
}
