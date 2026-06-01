using Glamour.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Controllers;

[Route("")]
public class SeoController(ProdutoService produtoService, CategoriaService categoriaService) : Controller
{
    [HttpGet("sitemap.xml")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> Sitemap()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var (produtos, _) = await produtoService.ListarAsync(null, null, null, null, null, "novo", true, 1, 500);
        var categorias = await categoriaService.ObterAtivasAsync();

        var urls = new List<(string loc, string? lastmod, string changefreq, string priority)>
        {
            (baseUrl + "/", null, "weekly", "1.0"),
            (baseUrl + "/catalogo", null, "daily", "0.9"),
            (baseUrl + "/quiz", null, "monthly", "0.7"),
        };

        foreach (var cat in categorias)
            urls.Add((baseUrl + $"/catalogo?categoriaId={cat.Id}", null, "weekly", "0.8"));

        foreach (var p in produtos)
            urls.Add((baseUrl + $"/produto/{p.Slug}", null, "weekly", "0.8"));

        var xml = new System.Text.StringBuilder();
        xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        xml.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
        foreach (var (loc, lastmod, changefreq, priority) in urls)
        {
            xml.AppendLine("  <url>");
            xml.AppendLine($"    <loc>{loc}</loc>");
            if (lastmod != null) xml.AppendLine($"    <lastmod>{lastmod}</lastmod>");
            xml.AppendLine($"    <changefreq>{changefreq}</changefreq>");
            xml.AppendLine($"    <priority>{priority}</priority>");
            xml.AppendLine("  </url>");
        }
        xml.AppendLine("</urlset>");

        return Content(xml.ToString(), "application/xml");
    }

    [HttpGet("robots.txt")]
    [ResponseCache(Duration = 86400)]
    public IActionResult Robots()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var content = $"User-agent: *\nAllow: /\nDisallow: /admin\nDisallow: /conta\nDisallow: /checkout\nDisallow: /carrinho\n\nSitemap: {baseUrl}/sitemap.xml\n";
        return Content(content, "text/plain");
    }
}
