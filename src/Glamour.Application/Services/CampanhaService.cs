using Glamour.Application.DTOs;
using Glamour.Domain.Entities;
using Glamour.Domain.Interfaces;

namespace Glamour.Application.Services;

public class CampanhaService(IRepository<Campanha> repo)
{
    public async Task<IEnumerable<CampanhaDto>> ListarAtivasAsync() =>
        (await repo.BuscarAsync(c => c.Ativa)).OrderBy(c => c.Ordem).ThenByDescending(c => c.CriadoEm).Select(Map);

    public async Task<IEnumerable<CampanhaDto>> ListarTodasAsync() =>
        (await repo.ObterTodosAsync()).OrderBy(c => c.Ordem).ThenByDescending(c => c.CriadoEm).Select(Map);

    public async Task<CampanhaDto?> ObterAsync(Guid id)
    {
        var campanha = await repo.ObterPorIdAsync(id);
        return campanha == null ? null : Map(campanha);
    }

    public async Task<Guid> CriarAsync(string titulo, string? subtitulo, string? link, int ordem)
    {
        var campanha = new Campanha(titulo, subtitulo, link, ordem);
        await repo.AdicionarAsync(campanha);
        await repo.SalvarAsync();
        return campanha.Id;
    }

    public async Task AtualizarAsync(Guid id, string titulo, string? subtitulo, string? link, int ordem, bool ativa)
    {
        var campanha = await repo.ObterPorIdAsync(id);
        if (campanha == null) return;
        campanha.Atualizar(titulo, subtitulo, link, ordem, ativa);
        await repo.AtualizarAsync(campanha);
        await repo.SalvarAsync();
    }

    public async Task DefinirImagemAsync(Guid id, string? url)
    {
        var campanha = await repo.ObterPorIdAsync(id);
        if (campanha == null) return;
        campanha.DefinirImagem(url);
        await repo.AtualizarAsync(campanha);
        await repo.SalvarAsync();
    }

    public async Task RemoverAsync(Guid id)
    {
        var campanha = await repo.ObterPorIdAsync(id);
        if (campanha == null) return;
        await repo.RemoverAsync(campanha);
        await repo.SalvarAsync();
    }

    private static CampanhaDto Map(Campanha c) =>
        new(c.Id, c.Titulo, c.Subtitulo, c.ImagemUrl, c.Link, c.Ordem, c.Ativa);
}
