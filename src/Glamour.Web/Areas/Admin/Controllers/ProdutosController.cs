using Glamour.Application.DTOs;
using Glamour.Application.Services;
using Glamour.Domain.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("admin/produtos")]
public class ProdutosController(ProdutoService produtoService, CategoriaService categoriaService, NotificacaoContext notificacoes) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? busca, int pagina = 1)
    {
        var (produtos, total) = await produtoService.ListarAsync(busca, null, null, null, null, "novo", true, pagina, 20);
        ViewBag.Produtos = produtos;
        ViewBag.Total = total;
        ViewBag.PaginaAtual = pagina;
        ViewBag.TotalPaginas = (int)Math.Ceiling(total / 20.0);
        return View();
    }

    [HttpGet("criar")]
    public async Task<IActionResult> Criar()
    {
        ViewBag.Categorias = await categoriaService.ObterAtivasAsync();
        return View();
    }

    [HttpPost("criar")]
    public async Task<IActionResult> Criar(CriarProdutoDto dto)
    {
        var id = await produtoService.CriarAsync(dto);
        if (!notificacoes.Valido)
        {
            foreach (var n in notificacoes.Notificacoes)
                ModelState.AddModelError(n.Campo, n.Mensagem);
            ViewBag.Categorias = await categoriaService.ObterAtivasAsync();
            return View(dto);
        }
        TempData["Sucesso"] = "Produto criado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("editar/{id}")]
    public async Task<IActionResult> Editar(Guid id)
    {
        var produto = await produtoService.ObterPorIdAsync(id);
        if (produto == null) return NotFound();

        var dto = new AtualizarProdutoDto(
            produto.Id, produto.Nome, produto.Slug, produto.Descricao,
            produto.Preco, produto.PrecoPromo, produto.Estoque,
            Guid.Parse(produto.CategoriaId), produto.Marca,
            produto.Volume, produto.Genero, produto.Destaque);

        ViewBag.Categorias = await categoriaService.ObterAtivasAsync();
        ViewBag.UrlImagem = produto.Imagens.FirstOrDefault(i => i.Principal)?.Url
                         ?? produto.Imagens.FirstOrDefault()?.Url;
        return View(dto);
    }

    [HttpPost("editar")]
    public async Task<IActionResult> Editar(AtualizarProdutoDto dto)
    {
        await produtoService.AtualizarAsync(dto);
        if (!notificacoes.Valido)
        {
            foreach (var n in notificacoes.Notificacoes)
                ModelState.AddModelError(n.Campo, n.Mensagem);
            ViewBag.Categorias = await categoriaService.ObterAtivasAsync();
            return View(dto);
        }
        TempData["Sucesso"] = "Produto atualizado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("remover/{id}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        await produtoService.RemoverAsync(id);
        TempData["Sucesso"] = "Produto desativado.";
        return RedirectToAction(nameof(Index));
    }
}
