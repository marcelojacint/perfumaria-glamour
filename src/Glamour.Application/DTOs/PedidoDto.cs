using Glamour.Domain.Enums;

namespace Glamour.Application.DTOs;

public record PedidoDto(
    Guid Id, string UsuarioId, StatusPedido Status,
    decimal Subtotal, decimal Frete, decimal Desconto, decimal Total,
    TipoEntrega TipoEntrega, MetodoPagamento MetodoPagamento,
    string? CupomCodigo, string? CodigoRastreio, string? Observacoes,
    DateTime CriadoEm,
    EnderecoDto? Endereco,
    IEnumerable<PedidoItemDto> Itens);

public record PedidoItemDto(
    Guid ProdutoId, string NomeProduto, int Quantidade, decimal PrecoUnitario, decimal Subtotal);

public record CriarPedidoDto(
    TipoEntrega TipoEntrega,
    MetodoPagamento MetodoPagamento,
    Guid? EnderecoId,
    string? CupomCodigo,
    string? Observacoes,
    string CarrinhoId);

public record AtualizarStatusPedidoDto(Guid PedidoId, StatusPedido NovoStatus);
