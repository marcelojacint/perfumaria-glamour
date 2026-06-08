using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Glamour.Domain.Entities;
using Glamour.Domain.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Glamour.Web.Services;

public class ImagemService(
    IWebHostEnvironment env,
    IConfiguration config,
    IProdutoRepository produtoRepo,
    IRepository<ProdutoImagem> imagemRepo)
{
    private static readonly string[] _extensoesPermitidas = [".jpg", ".jpeg", ".png", ".webp", ".avif"];
    private const long TamanhoMaxBytes = 5 * 1024 * 1024;
    private const string OtimizacaoCloudinary = "f_auto,q_auto,w_1200,c_limit";

    public async Task<(bool ok, string? urlOuErro)> SalvarImagemAsync(IFormFile arquivo, string subpasta = "produtos")
    {
        var ext = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
        if (!_extensoesPermitidas.Contains(ext))
            return (false, "Formato inválido. Use JPG, PNG ou WebP.");

        if (arquivo.Length > TamanhoMaxBytes)
            return (false, "Arquivo muito grande. Máximo 5 MB.");

        var cloudinaryUrl = config["CLOUDINARY_URL"];
        if (!string.IsNullOrWhiteSpace(cloudinaryUrl))
            return await SalvarNoCloudinaryAsync(arquivo, subpasta, cloudinaryUrl);

        var nomeArquivo = $"{Guid.NewGuid():N}{ext}";
        var pasta = Path.Combine(env.WebRootPath, "uploads", subpasta);
        Directory.CreateDirectory(pasta);
        var caminho = Path.Combine(pasta, nomeArquivo);

        await using var stream = File.Create(caminho);
        await arquivo.CopyToAsync(stream);

        return (true, $"/uploads/{subpasta}/{nomeArquivo}");
    }

    private static async Task<(bool ok, string? urlOuErro)> SalvarNoCloudinaryAsync(IFormFile arquivo, string subpasta, string cloudinaryUrl)
    {
        try
        {
            var cloudinary = new Cloudinary(cloudinaryUrl) { Api = { Secure = true } };

            await using var stream = arquivo.OpenReadStream();
            var resultado = await cloudinary.UploadAsync(new ImageUploadParams
            {
                File = new FileDescription(arquivo.FileName, stream),
                Folder = $"glamour/{subpasta}",
                UniqueFilename = true,
                Overwrite = false
            });

            if (resultado.Error != null || resultado.SecureUrl == null)
                return (false, "Não foi possível enviar a imagem. Tente novamente.");

            var url = resultado.SecureUrl.ToString()
                .Replace("/upload/", $"/upload/{OtimizacaoCloudinary}/");
            return (true, url);
        }
        catch
        {
            return (false, "Não foi possível enviar a imagem. Tente novamente.");
        }
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
