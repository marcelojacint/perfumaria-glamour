using Glamour.Domain.Entities;
using Glamour.Domain.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Glamour.Web.Services;

public class ImagemService(IWebHostEnvironment env, IProdutoRepository produtoRepo, IRepository<ProdutoImagem> imagemRepo)
{
    private static readonly string[] _extensoesPermitidas = [".jpg", ".jpeg", ".png", ".webp", ".avif"];
    private const long TamanhoMaxBytes = 5 * 1024 * 1024;

    public async Task<(bool ok, string? urlOuErro)> SalvarImagemAsync(IFormFile arquivo)
    {
        var ext = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
        if (!_extensoesPermitidas.Contains(ext))
            return (false, "Formato inválido. Use JPG, PNG ou WebP.");

        if (arquivo.Length > TamanhoMaxBytes)
            return (false, "Arquivo muito grande. Máximo 5 MB.");

        var nomeArquivo = $"{Guid.NewGuid():N}{ext}";
        var pasta = Path.Combine(env.WebRootPath, "uploads", "produtos");
        Directory.CreateDirectory(pasta);
        var caminho = Path.Combine(pasta, nomeArquivo);

        await using var stream = File.Create(caminho);
        await arquivo.CopyToAsync(stream);

        return (true, $"/uploads/produtos/{nomeArquivo}");
    }

    public async Task AdicionarImagemProdutoAsync(Guid produtoId, string url, bool principal = false)
    {
        var produto = await produtoRepo.ObterPorIdAsync(produtoId);
        if (produto == null) return;

        var existentes = (await imagemRepo.BuscarAsync(i => i.ProdutoId == produtoId)).ToList();
        var virarPrincipal = principal || existentes.Count == 0;

        if (virarPrincipal)
        {
            foreach (var img in existentes.Where(i => i.Principal))
            {
                img.RemoverPrincipal();
                await imagemRepo.AtualizarAsync(img);
            }
        }

        var ordem = existentes.Count + 1;
        await imagemRepo.AdicionarAsync(new ProdutoImagem(produtoId, url, ordem, virarPrincipal));
        await imagemRepo.SalvarAsync();
    }

    public void ExcluirArquivoLocal(string url)
    {
        if (!url.StartsWith("/uploads/")) return;
        var caminho = Path.Combine(env.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(caminho)) File.Delete(caminho);
    }
}
