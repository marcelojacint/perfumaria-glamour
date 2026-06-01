using Glamour.Application.Services;
using Glamour.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Controllers;

public class CarrinhoController(ICarrinhoService carrinhoService, ProdutoService produtoService) : Controller
{
    private string CarrinhoId => HttpContext.Session.GetString("CarrinhoId") ?? CriarCarrinhoId();

    private string CriarCarrinhoId()
    {
        var id = Guid.NewGuid().ToString();
        HttpContext.Session.SetString("CarrinhoId", id);
        return id;
    }

    [Route("carrinho")]
    public async Task<IActionResult> Index()
    {
        var itens = await carrinhoService.ObterCarrinhoAsync(CarrinhoId);
        return View(itens);
    }

    [HttpPost, Route("carrinho/adicionar")]
    public async Task<IActionResult> Adicionar(Guid produtoId, int quantidade = 1)
    {
        var produto = await produtoService.ObterPorSlugAsync("");
        // Busca produto pelo Id
        var itens = await carrinhoService.ObterCarrinhoAsync(CarrinhoId);
        var existente = itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (existente != null)
        {
            await carrinhoService.AtualizarQuantidadeAsync(CarrinhoId, produtoId, existente.Quantidade + quantidade);
        }
        else
        {
            // Precisamos buscar o produto pelo id — simplificado aqui
            await carrinhoService.AdicionarItemAsync(CarrinhoId,
                new ItemCarrinho(produtoId, "Produto", null, 0, quantidade));
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Route("carrinho/remover/{produtoId}")]
    public async Task<IActionResult> Remover(Guid produtoId)
    {
        await carrinhoService.RemoverItemAsync(CarrinhoId, produtoId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Route("carrinho/atualizar")]
    public async Task<IActionResult> Atualizar(Guid produtoId, int quantidade)
    {
        await carrinhoService.AtualizarQuantidadeAsync(CarrinhoId, produtoId, quantidade);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, Route("carrinho/quantidade")]
    public async Task<IActionResult> Quantidade()
    {
        var qtd = await carrinhoService.ObterQuantidadeItensAsync(CarrinhoId);
        return Json(qtd);
    }
}
