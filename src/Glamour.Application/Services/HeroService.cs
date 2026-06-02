using Glamour.Application.DTOs;
using Glamour.Domain.Entities;
using Glamour.Domain.Interfaces;

namespace Glamour.Application.Services;

public class HeroService(IRepository<ConfiguracaoHero> repo)
{
    public async Task<HeroDto> ObterAsync() => Map(await ObterOuCriarAsync());

    public async Task AtualizarAsync(string? eyebrow, string? titulo, string? tituloDestaque,
        string? subtitulo, string? corDestaque, string? corTexto)
    {
        var configuracao = await ObterOuCriarAsync();
        configuracao.Atualizar(eyebrow, titulo, tituloDestaque, subtitulo, corDestaque, corTexto);
        await repo.AtualizarAsync(configuracao);
        await repo.SalvarAsync();
    }

    public async Task DefinirImagemAsync(string? url)
    {
        var configuracao = await ObterOuCriarAsync();
        configuracao.DefinirImagem(url);
        await repo.AtualizarAsync(configuracao);
        await repo.SalvarAsync();
    }

    public async Task RestaurarPadraoAsync()
    {
        var configuracao = await ObterOuCriarAsync();
        configuracao.RestaurarPadrao();
        await repo.AtualizarAsync(configuracao);
        await repo.SalvarAsync();
    }

    private async Task<ConfiguracaoHero> ObterOuCriarAsync()
    {
        var existente = await repo.ObterPorIdAsync(ConfiguracaoHero.IdSingleton);
        if (existente != null) return existente;

        var nova = ConfiguracaoHero.Padrao();
        await repo.AdicionarAsync(nova);
        await repo.SalvarAsync();
        return nova;
    }

    private static HeroDto Map(ConfiguracaoHero c) =>
        new(c.Eyebrow, c.Titulo, c.TituloDestaque, c.Subtitulo, c.CorDestaque, c.CorTexto, c.ImagemFundoUrl);
}
