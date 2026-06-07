using Glamour.Application.DTOs;
using Glamour.Application.Services;
using Glamour.Domain.Enums;
using Glamour.Domain.Interfaces;
using Glamour.Domain.Notifications;
using Glamour.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Controllers;

[Authorize]
[Route("checkout")]
public class CheckoutController(
    PedidoService pedidoService,
    EnderecoService enderecoService,
    ICarrinhoService carrinhoService,
    ProdutoService produtoService,
    FreteService freteService,
    ICupomRepository cupomRepo,
    UserManager<ApplicationUser> userManager,
    NotificacaoContext notificacoes) : Controller
{
    private string CarrinhoId => HttpContext.ObterCarrinhoId();
    private string UsuarioId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

    private async Task<(decimal promocao, decimal normal)> CalcularSubtotaisAsync(IEnumerable<Glamour.Domain.Interfaces.ItemCarrinho> itens)
    {
        var produtos = (await produtoService.ObterListagemPorIdsAsync(itens.Select(i => i.ProdutoId)))
            .ToDictionary(p => p.Id);
        decimal promocao = 0, normal = 0;
        foreach (var item in itens)
        {
            if (!produtos.TryGetValue(item.ProdutoId, out var p)) continue;
            var valor = (p.PrecoPromo ?? p.Preco) * item.Quantidade;
            if (p.PrecoPromo.HasValue) promocao += valor; else normal += valor;
        }
        return (promocao, normal);
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var itens = await carrinhoService.ObterCarrinhoAsync(CarrinhoId);
        if (!itens.Any()) return RedirectToAction("Index", "Carrinho");

        var usuario = await userManager.GetUserAsync(User);
        var (subtotalPromocao, subtotalNormal) = await CalcularSubtotaisAsync(itens);
        ViewBag.TelefoneCliente = usuario?.Telefone ?? usuario?.PhoneNumber ?? "";
        ViewBag.SubtotalPromocao = subtotalPromocao;
        ViewBag.SubtotalNormal = subtotalNormal;
        ViewBag.TemPromocao = subtotalPromocao > 0;
        ViewBag.TemNormal = subtotalNormal > 0;
        ViewBag.Enderecos = await enderecoService.ListarPorUsuarioAsync(UsuarioId);
        return View(itens);
    }

    [HttpGet("calcular-frete")]
    public async Task<IActionResult> CalcularFrete(string? cidade, string? uf)
    {
        var itens = await carrinhoService.ObterCarrinhoAsync(CarrinhoId);
        var subtotal = itens.Sum(i => i.Preco * i.Quantidade);
        var resultado = freteService.Calcular(cidade, uf, subtotal);
        return Json(resultado);
    }

    [HttpGet("validar-cupom")]
    public async Task<IActionResult> ValidarCupom(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return Json(new { valido = false, desconto = 0m, mensagem = "" });

        var itens = await carrinhoService.ObterCarrinhoAsync(CarrinhoId);
        var subtotal = itens.Sum(i => i.Preco * i.Quantidade);

        var cupom = await cupomRepo.ObterPorCodigoAsync(codigo.Trim());
        if (cupom is null || !cupom.Valido)
            return Json(new { valido = false, desconto = 0m, mensagem = "Cupom inválido ou expirado." });

        var desconto = cupom.CalcularDesconto(subtotal);
        if (desconto <= 0)
        {
            var minimo = cupom.ValorMinimoPedido ?? 0m;
            return Json(new { valido = false, desconto = 0m, mensagem = $"Cupom válido para pedidos a partir de R$ {minimo:N2}." });
        }

        var detalhe = cupom.TipoDesconto == TipoDesconto.Percentual
            ? $"Cupom aplicado: {cupom.Valor:0.##}% de desconto."
            : "Cupom aplicado.";
        return Json(new { valido = true, desconto, mensagem = detalhe });
    }

    [HttpPost]
    public async Task<IActionResult> Finalizar(
        string tipoEntrega, string metodoPagamento,
        string? enderecoId,

        string? cep, string? logradouro, string? numero,
        string? complemento, string? bairro, string? cidade, string? uf,
        string? cupomCodigo, string? observacoes, string? telefone, string? metodoPagamentoPromocao)
    {
        var usuario = await userManager.GetUserAsync(User);
        if (usuario == null) return Challenge();

        var telefoneInformado = (telefone ?? "").Trim();
        var telefoneCadastrado = usuario.Telefone ?? usuario.PhoneNumber;

        if (string.IsNullOrWhiteSpace(telefoneInformado) && string.IsNullOrWhiteSpace(telefoneCadastrado))
        {
            TempData["Erro"] = "Informe um telefone para contato. Precisamos dele para confirmar seu pedido.";
            return RedirectToAction(nameof(Index));
        }

        if (!string.IsNullOrWhiteSpace(telefoneInformado) && telefoneInformado != usuario.Telefone)
        {
            usuario.Telefone = telefoneInformado;
            await userManager.UpdateAsync(usuario);
        }

        if (!Enum.TryParse<TipoEntrega>(tipoEntrega, out var tipo))
            tipo = TipoEntrega.Entrega;

        if (!Enum.TryParse<MetodoPagamento>(metodoPagamento, out var metodo))
            metodo = MetodoPagamento.Dinheiro;

        Guid? enderecoFinalId = null;

        if (tipo == TipoEntrega.Entrega)
        {

            if (Guid.TryParse(enderecoId, out var idSalvo) && idSalvo != Guid.Empty)
            {
                enderecoFinalId = idSalvo;
            }

            else if (!string.IsNullOrWhiteSpace(logradouro) && !string.IsNullOrWhiteSpace(cep))
            {
                var novoEndereco = new CriarEnderecoDto(
                    cep!, logradouro!, numero ?? "S/N",
                    complemento, bairro ?? "", cidade ?? "", uf ?? "SP", null);
                enderecoFinalId = await enderecoService.CriarAsync(UsuarioId, novoEndereco);
            }
            else
            {
                TempData["Erro"] = "Informe o endereço de entrega.";
                return RedirectToAction(nameof(Index));
            }
        }

        MetodoPagamento? metodoPromo = Enum.TryParse<MetodoPagamento>(metodoPagamentoPromocao, out var mp) ? mp : null;
        var dto = new CriarPedidoDto(tipo, metodo, enderecoFinalId, cupomCodigo, observacoes, CarrinhoId, metodoPromo);
        var pedidoId = await pedidoService.CriarAsync(dto, UsuarioId, usuario.Nome);

        if (pedidoId == Guid.Empty)
        {
            foreach (var n in notificacoes.Notificacoes)
                TempData["Erro"] = n.Mensagem;
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Confirmacao), new { id = pedidoId });
    }

    [HttpGet("confirmacao/{id}")]
    public async Task<IActionResult> Confirmacao(Guid id)
    {
        var pedido = await pedidoService.ObterAsync(id);
        if (pedido == null) return NotFound();
        return View(pedido);
    }
}
