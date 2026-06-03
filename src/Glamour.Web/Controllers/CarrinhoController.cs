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
        var ehAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        var produto = await produtoService.ObterPorIdAsync(produtoId);
        if (produto == null)
            return ehAjax ? Json(new { ok = false }) : RedirectToAction(nameof(Index));

        var imagem = produto.Imagens.FirstOrDefault(i => i.Principal)?.Url
                  ?? produto.Imagens.FirstOrDefault()?.Url;

        var itens = await carrinhoService.ObterCarrinhoAsync(CarrinhoId);
        var existente = itens.FirstOrDefault(i => i.ProdutoId == produtoId);

        if (existente != null)
            await carrinhoService.AtualizarQuantidadeAsync(CarrinhoId, produtoId, existente.Quantidade + quantidade);
        else
            await carrinhoService.AdicionarItemAsync(CarrinhoId,
                new ItemCarrinho(produtoId, produto.Nome, imagem, produto.PrecoPromo ?? produto.Preco, quantidade));

        if (ehAjax)
        {
            var total = await carrinhoService.ObterQuantidadeItensAsync(CarrinhoId);
            return Json(new { ok = true, quantidade = total, mensagem = $"\"{produto.Nome}\" adicionado ao carrinho!" });
        }

        TempData["Sucesso"] = $"\"{produto.Nome}\" adicionado ao carrinho!";
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
