using Glamour.Application.DTOs;
using Glamour.Domain.Entities;
using Glamour.Domain.Interfaces;
using Glamour.Domain.Notifications;

namespace Glamour.Application.Services;

public class EnderecoService(IRepository<Endereco> enderecoRepo, NotificacaoContext notificacoes)
{
    public async Task<IEnumerable<EnderecoDto>> ListarPorUsuarioAsync(string usuarioId)
    {
        var enderecos = await enderecoRepo.BuscarAsync(e => e.UsuarioId == usuarioId);
        return enderecos.Select(MapDto);
    }

    public async Task<Guid> CriarAsync(string usuarioId, CriarEnderecoDto dto)
    {
        var endereco = new Endereco(usuarioId, dto.CEP, dto.Logradouro, dto.Numero,
            dto.Complemento, dto.Bairro, dto.Cidade, dto.UF, dto.Apelido);

        var existentes = await enderecoRepo.BuscarAsync(e => e.UsuarioId == usuarioId);
        if (!existentes.Any()) endereco.DefinirComoPrincipal();

        await enderecoRepo.AdicionarAsync(endereco);
        await enderecoRepo.SalvarAsync();
        return endereco.Id;
    }

    public async Task<bool> DefinirPrincipalAsync(Guid enderecoId, string usuarioId)
    {
        var enderecos = await enderecoRepo.BuscarAsync(e => e.UsuarioId == usuarioId);
        foreach (var e in enderecos) { e.RemoverPrincipal(); await enderecoRepo.AtualizarAsync(e); }

        var principal = enderecos.FirstOrDefault(e => e.Id == enderecoId);
        if (principal == null) { notificacoes.Adicionar("Id", "Endereço não encontrado."); return false; }

        principal.DefinirComoPrincipal();
        await enderecoRepo.AtualizarAsync(principal);
        await enderecoRepo.SalvarAsync();
        return true;
    }

    private static EnderecoDto MapDto(Endereco e) =>
        new(e.Id, e.CEP, e.Logradouro, e.Numero, e.Complemento, e.Bairro, e.Cidade, e.UF, e.Apelido, e.Principal);
}
