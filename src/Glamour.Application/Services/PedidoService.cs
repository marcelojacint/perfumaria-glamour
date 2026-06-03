using Glamour.Application.DTOs;
using Glamour.Domain.Entities;
using Glamour.Domain.Enums;
using Glamour.Domain.Interfaces;
using Glamour.Domain.Notifications;

namespace Glamour.Application.Services;

public class PedidoService(
    IPedidoRepository pedidoRepo,
    IProdutoRepository produtoRepo,
    ICupomRepository cupomRepo,
    ICarrinhoService carrinhoService,
    IFidelidadeService fidelidadeService,
    IRepository<Endereco> enderecoRepo,
    FreteService freteService,
    NotificacaoContext notificacoes)
{
    public async Task<PedidoDto?> ObterAsync(Guid pedidoId)
    {
        var pedido = await pedidoRepo.ObterComItensAsync(pedidoId);
        return pedido == null ? null : MapDto(pedido);
    }

    public async Task<IEnumerable<PedidoDto>> ObterPorUsuarioAsync(string usuarioId) =>
        (await pedidoRepo.ObterPorUsuarioAsync(usuarioId)).Select(MapDto);

    public async Task<Guid> CriarAsync(CriarPedidoDto dto, string usuarioId)
    {
        var itensCarrinho = await carrinhoService.ObterCarrinhoAsync(dto.CarrinhoId);
        if (!itensCarrinho.Any())
        {
            notificacoes.Adicionar("Carrinho", "Carrinho está vazio.");
            return Guid.Empty;
        }

        var ehCartao = dto.MetodoPagamento is MetodoPagamento.CartaoCredito or MetodoPagamento.CartaoDebito;
        if (ehCartao)
        {
            foreach (var item in itensCarrinho)
            {
                var produtoPromo = await produtoRepo.ObterPorIdAsync(item.ProdutoId);
                if (produtoPromo?.PrecoPromo != null)
                {
                    notificacoes.Adicionar("Pagamento", "Itens em promoção só podem ser pagos com PIX ou dinheiro.");
                    return Guid.Empty;
                }
            }
        }

        if (dto.TipoEntrega == TipoEntrega.Entrega && dto.EnderecoId == null)
        {
            notificacoes.Adicionar("Endereco", "Endereço de entrega é obrigatório.");
            return Guid.Empty;
        }

        decimal desconto = 0;
        Cupom? cupom = null;
        decimal subtotalEstimado = itensCarrinho.Sum(i => i.Preco * i.Quantidade);

        Endereco? enderecoEntrega = null;
        if (dto.TipoEntrega == TipoEntrega.Entrega)
        {
            enderecoEntrega = await enderecoRepo.ObterPorIdAsync(dto.EnderecoId!.Value);
            if (enderecoEntrega == null)
            {
                notificacoes.Adicionar("Endereco", "Endereço de entrega não encontrado.");
                return Guid.Empty;
            }

            if (!freteService.MesmaLocalidade(enderecoEntrega.Cidade, enderecoEntrega.UF))
            {
                notificacoes.Adicionar("Frete",
                    "Não realizamos entrega automática para esse endereço. Entre em contato com a loja ou escolha retirada na loja.");
                return Guid.Empty;
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.CupomCodigo))
        {
            cupom = await cupomRepo.ObterPorCodigoAsync(dto.CupomCodigo);
            if (cupom == null || !cupom.Valido)
                notificacoes.Adicionar("Cupom", "Cupom inválido ou expirado.");
            else
                desconto = cupom.CalcularDesconto(subtotalEstimado);
        }

        var pedido = new Pedido(usuarioId, dto.TipoEntrega, dto.MetodoPagamento,
            dto.EnderecoId, frete: 0, dto.CupomCodigo, dto.Observacoes);

        foreach (var item in itensCarrinho)
        {
            var produto = await produtoRepo.ObterPorIdAsync(item.ProdutoId);
            if (produto == null || !produto.DebitarEstoque(item.Quantidade))
            {
                notificacoes.Adicionar("Estoque", $"Produto '{item.Nome}' sem estoque suficiente.");
                return Guid.Empty;
            }

            pedido.AdicionarItem(new PedidoItem(pedido.Id, produto.Id, produto.Nome, item.Quantidade, produto.PrecoEfetivo));
            await produtoRepo.AtualizarAsync(produto);
        }

        if (enderecoEntrega != null)
        {
            var resultadoFrete = freteService.Calcular(enderecoEntrega.Cidade, enderecoEntrega.UF, pedido.Subtotal);
            pedido.DefinirFrete(resultadoFrete.Valor);
        }

        if (desconto > 0) pedido.AplicarDesconto(desconto);

        await pedidoRepo.AdicionarAsync(pedido);
        await pedidoRepo.SalvarAsync();

        cupom?.IncrementarUso();
        if (cupom != null) await cupomRepo.SalvarAsync();

        await carrinhoService.LimparCarrinhoAsync(dto.CarrinhoId);
        return pedido.Id;
    }

    public async Task<Guid> RegistrarVendaLojaAsync(RegistrarVendaLojaDto dto, string adminId)
    {
        var itens = dto.Itens.Where(i => i.Quantidade > 0).ToList();
        if (itens.Count == 0)
        {
            notificacoes.Adicionar("Itens", "Adicione ao menos um produto à venda.");
            return Guid.Empty;
        }

        var pedido = new Pedido(adminId, TipoEntrega.RetiradaNaLoja, dto.MetodoPagamento,
            enderecoId: null, frete: 0, cupomCodigo: null, observacoes: dto.Observacoes);

        foreach (var item in itens)
        {
            var produto = await produtoRepo.ObterPorIdAsync(item.ProdutoId);
            if (produto == null)
            {
                notificacoes.Adicionar("Produto", "Produto não encontrado.");
                return Guid.Empty;
            }
            if (!produto.DebitarEstoque(item.Quantidade))
            {
                notificacoes.Adicionar("Estoque", $"'{produto.Nome}' sem estoque suficiente (disponível: {produto.Estoque}).");
                return Guid.Empty;
            }
            pedido.AdicionarItem(new PedidoItem(pedido.Id, produto.Id, produto.Nome, item.Quantidade, produto.PrecoEfetivo));
            await produtoRepo.AtualizarAsync(produto);
        }

        if (dto.Desconto > 0) pedido.AplicarDesconto(dto.Desconto);
        pedido.RegistrarVendaLoja(dto.NomeCliente);

        await pedidoRepo.AdicionarAsync(pedido);
        await pedidoRepo.SalvarAsync();
        return pedido.Id;
    }

    public async Task<bool> AtualizarStatusAsync(AtualizarStatusPedidoDto dto)
    {
        var pedido = await pedidoRepo.ObterComItensAsync(dto.PedidoId);
        if (pedido == null) { notificacoes.Adicionar("Id", "Pedido não encontrado."); return false; }

        var statusAnterior = pedido.Status;
        pedido.AtualizarStatus(dto.NovoStatus);
        await pedidoRepo.AtualizarAsync(pedido);
        await pedidoRepo.SalvarAsync();

        if (dto.NovoStatus == StatusPedido.Entregue && statusAnterior != StatusPedido.Entregue)
            await fidelidadeService.CreditarPontosAsync(pedido.UsuarioId, pedido.Total);

        return true;
    }

    private static PedidoDto MapDto(Pedido p) => new(
        p.Id, p.UsuarioId, p.Status,
        p.Subtotal, p.Frete, p.Desconto, p.Total,
        p.TipoEntrega, p.MetodoPagamento,
        p.CupomCodigo, p.CodigoRastreio, p.Observacoes,
        p.CriadoEm,
        p.Endereco == null ? null : new EnderecoDto(
            p.Endereco.Id, p.Endereco.CEP, p.Endereco.Logradouro, p.Endereco.Numero,
            p.Endereco.Complemento, p.Endereco.Bairro, p.Endereco.Cidade, p.Endereco.UF,
            p.Endereco.Apelido, p.Endereco.Principal),
        p.Itens.Select(i => new PedidoItemDto(i.ProdutoId, i.NomeProduto, i.Quantidade, i.PrecoUnitario, i.Subtotal)),
        p.Origem, p.NomeCliente);
}
