using Glamour.Domain.Enums;

namespace Glamour.Application.DTOs;

public record PedidoDto(
    Guid Id, string UsuarioId, StatusPedido Status,
    decimal Subtotal, decimal Frete, decimal Desconto, decimal Total,
    TipoEntrega TipoEntrega, MetodoPagamento MetodoPagamento,
    string? CupomCodigo, string? CodigoRastreio, string? Observacoes,
    DateTime CriadoEm,
    EnderecoDto? Endereco,
    IEnumerable<PedidoItemDto> Itens,
    OrigemPedido Origem = OrigemPedido.Site,
    string? NomeCliente = null,
    MetodoPagamento? MetodoPagamentoPromocao = null,
    decimal ValorEmPromocao = 0);

public record ItemVendaLojaDto(Guid ProdutoId, int Quantidade);

public record RegistrarVendaLojaDto(
    IEnumerable<ItemVendaLojaDto> Itens,
    MetodoPagamento MetodoPagamento,
    string? NomeCliente,
    decimal Desconto,
    string? Observacoes);

public record PedidoItemDto(
    Guid ProdutoId, string NomeProduto, int Quantidade, decimal PrecoUnitario, decimal Subtotal);

public record CriarPedidoDto(
    TipoEntrega TipoEntrega,
    MetodoPagamento MetodoPagamento,
    Guid? EnderecoId,
    string? CupomCodigo,
    string? Observacoes,
    string CarrinhoId,
    MetodoPagamento? MetodoPagamentoPromocao = null);

public record AtualizarStatusPedidoDto(Guid PedidoId, StatusPedido NovoStatus);
