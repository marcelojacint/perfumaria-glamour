using Microsoft.AspNetCore.Identity;

namespace Glamour.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string Nome { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public bool Ativo { get; set; } = true;
    public int PontosLoyalty { get; set; }
}
