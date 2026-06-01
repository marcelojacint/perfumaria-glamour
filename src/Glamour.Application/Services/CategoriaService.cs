using Glamour.Application.DTOs;
using Glamour.Domain.Entities;
using Glamour.Domain.Interfaces;
using Glamour.Domain.Notifications;

namespace Glamour.Application.Services;

public class CategoriaService(ICategoriaRepository repo, NotificacaoContext notificacoes)
{
    public async Task<IEnumerable<CategoriaDto>> ObterAtivasAsync() =>
        (await repo.ObterAtivasAsync()).Select(MapDto);

    public async Task<IEnumerable<CategoriaDto>> ObterTodasAsync() =>
        (await repo.ObterTodosAsync()).Select(MapDto);

    public async Task<Guid> CriarAsync(CriarCategoriaDto dto)
    {
        var categoria = new Categoria(dto.Nome, dto.Slug, dto.Ordem);
        await repo.AdicionarAsync(categoria);
        await repo.SalvarAsync();
        return categoria.Id;
    }

    public async Task<bool> AtualizarAsync(AtualizarCategoriaDto dto)
    {
        var categoria = await repo.ObterPorIdAsync(dto.Id);
        if (categoria == null) { notificacoes.Adicionar("Id", "Categoria não encontrada."); return false; }
        categoria.Atualizar(dto.Nome, dto.Slug, dto.Ordem);
        await repo.AtualizarAsync(categoria);
        await repo.SalvarAsync();
        return true;
    }

    public async Task<bool> ToggleAtivoAsync(Guid id)
    {
        var categoria = await repo.ObterPorIdAsync(id);
        if (categoria == null) { notificacoes.Adicionar("Id", "Categoria não encontrada."); return false; }
        if (categoria.Ativo) categoria.Desativar(); else categoria.Ativar();
        await repo.AtualizarAsync(categoria);
        await repo.SalvarAsync();
        return true;
    }

    private static CategoriaDto MapDto(Categoria c) => new(c.Id, c.Nome, c.Slug, c.Ativo, c.Ordem);
}
