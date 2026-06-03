using System.Security.Claims;
using Glamour.Application.DTOs;
using Glamour.Application.Services;
using Glamour.Domain.Enums;
using Glamour.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Controllers;

[Route("conta")]
public class ContaController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    PedidoService pedidoService,
    EnderecoService enderecoService) : Controller
{
    public const string ClaimMetodoLogin = "AuthMethod";
    public const string MetodoSenha = "Password";
    public const string MetodoGoogle = "Google";

    [HttpGet("registrar")]
    public async Task<IActionResult> Registrar()
    {
        ViewBag.GoogleDisponivel = (await signInManager.GetExternalAuthenticationSchemesAsync())
            .Any(s => s.Name == "Google");
        return View();
    }

    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar(RegistrarUsuarioDto dto)
    {
        if (dto.Senha != dto.ConfirmacaoSenha)
        {
            ModelState.AddModelError("", "As senhas não coincidem.");
            return View(dto);
        }

        var usuario = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            Nome = dto.Nome,
            Telefone = dto.Telefone,
            EmailConfirmed = true
        };

        var resultado = await userManager.CreateAsync(usuario, dto.Senha);
        if (!resultado.Succeeded)
        {
            foreach (var erro in resultado.Errors)
                ModelState.AddModelError("", erro.Description);
            return View(dto);
        }

        await userManager.AddToRoleAsync(usuario, RolesUsuario.Cliente);
        await signInManager.SignInAsync(usuario, isPersistent: false);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("login")]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        ViewBag.GoogleDisponivel = (await signInManager.GetExternalAuthenticationSchemesAsync())
            .Any(s => s.Name == "Google");
        return View();
    }

    [HttpPost("login")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
    {
        var resultado = await signInManager.PasswordSignInAsync(dto.Email, dto.Senha, dto.LembrarMe, lockoutOnFailure: true);
        if (resultado.Succeeded)
        {
            var usuario = await userManager.FindByEmailAsync(dto.Email);
            var ehAdmin = usuario != null && await userManager.IsInRoleAsync(usuario, RolesUsuario.Admin);

            if (usuario != null)
                await signInManager.SignInWithClaimsAsync(usuario, dto.LembrarMe, [new Claim(ClaimMetodoLogin, MetodoSenha)]);

            if (ehAdmin)
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        if (resultado.IsLockedOut)
            ModelState.AddModelError("", "Conta bloqueada. Aguarde alguns minutos.");
        else
            ModelState.AddModelError("", "E-mail ou senha inválidos.");

        return View(dto);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpPost("login-externo")]
    public IActionResult LoginExterno(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(LoginExternoCallback), "Conta", new { returnUrl });
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet("login-externo-callback")]
    public async Task<IActionResult> LoginExternoCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError != null)
        {
            TempData["Erro"] = $"Erro no provedor externo: {remoteError}";
            return RedirectToAction(nameof(Login));
        }

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            TempData["Erro"] = "Não foi possível obter informações do provedor externo.";
            return RedirectToAction(nameof(Login));
        }

        var resultado = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);

        if (resultado.Succeeded)
        {
            var existente = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (existente != null)
                await signInManager.SignInWithClaimsAsync(existente, isPersistent: true, [new Claim(ClaimMetodoLogin, MetodoGoogle)]);
            return await RedirecionarPosLoginAsync(info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email), returnUrl);
        }

        var email = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
        var nome = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Name) ?? email;

        if (string.IsNullOrEmpty(email))
        {
            TempData["Erro"] = "O provedor externo não forneceu um e-mail.";
            return RedirectToAction(nameof(Login));
        }

        var usuario = await userManager.FindByEmailAsync(email);
        if (usuario == null)
        {
            usuario = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Nome = nome ?? email,
                EmailConfirmed = true,
                Ativo = true
            };
            await userManager.CreateAsync(usuario);
            await userManager.AddToRoleAsync(usuario, RolesUsuario.Cliente);
        }

        await userManager.AddLoginAsync(usuario, info);
        await signInManager.SignInWithClaimsAsync(usuario, isPersistent: true, [new Claim(ClaimMetodoLogin, MetodoGoogle)]);

        return await RedirecionarPosLoginAsync(email, returnUrl);
    }

    private async Task<IActionResult> RedirecionarPosLoginAsync(string? email, string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        if (!string.IsNullOrEmpty(email))
        {
            var usuario = await userManager.FindByEmailAsync(email);
            if (usuario != null && await userManager.IsInRoleAsync(usuario, RolesUsuario.Admin))
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("meus-pedidos")]
    [Authorize]
    public async Task<IActionResult> MeusPedidos()
    {
        var usuarioId = userManager.GetUserId(User)!;
        var pedidos = await pedidoService.ObterPorUsuarioAsync(usuarioId);
        return View(pedidos);
    }

    [HttpGet("pedido/{id}")]
    [Authorize]
    public async Task<IActionResult> DetalhePedido(Guid id)
    {
        var pedido = await pedidoService.ObterAsync(id);
        if (pedido == null || pedido.UsuarioId != userManager.GetUserId(User))
            return NotFound();
        return View(pedido);
    }

    [HttpGet("perfil")]
    [Authorize]
    public async Task<IActionResult> Perfil()
    {
        var usuario = await userManager.GetUserAsync(User);
        if (usuario == null) return NotFound();
        var enderecos = await enderecoService.ListarPorUsuarioAsync(usuario.Id);
        ViewBag.Enderecos = enderecos;
        ViewBag.PontosLoyalty = usuario.PontosLoyalty;
        return View(usuario);
    }

    [HttpPost("perfil")]
    [Authorize]
    public async Task<IActionResult> AtualizarPerfil(string nome, string? telefone)
    {
        var usuario = await userManager.GetUserAsync(User);
        if (usuario == null) return NotFound();
        usuario.Nome = nome;
        usuario.Telefone = telefone;
        await userManager.UpdateAsync(usuario);
        TempData["Sucesso"] = "Perfil atualizado.";
        return RedirectToAction(nameof(Perfil));
    }

    [HttpPost("alterar-senha")]
    [Authorize]
    public async Task<IActionResult> AlterarSenha(AlterarSenhaDto dto)
    {
        var usuario = await userManager.GetUserAsync(User);
        if (usuario == null) return NotFound();

        if (dto.NovaSenha != dto.ConfirmacaoNovaSenha)
        {
            TempData["Erro"] = "A confirmação da nova senha não confere.";
            return RedirectToAction(nameof(Perfil));
        }

        var resultado = await userManager.ChangePasswordAsync(usuario, dto.SenhaAtual, dto.NovaSenha);
        if (!resultado.Succeeded)
        {
            TempData["Erro"] = "Senha atual incorreta.";
            return RedirectToAction(nameof(Perfil));
        }

        await signInManager.RefreshSignInAsync(usuario);
        TempData["Sucesso"] = "Senha alterada com sucesso.";
        return RedirectToAction(nameof(Perfil));
    }

    [HttpPost("enderecos/adicionar")]
    [Authorize]
    public async Task<IActionResult> AdicionarEndereco(CriarEnderecoDto dto)
    {
        var usuarioId = userManager.GetUserId(User)!;
        await enderecoService.CriarAsync(usuarioId, dto);
        TempData["Sucesso"] = "Endereço adicionado.";
        return RedirectToAction(nameof(Perfil));
    }

    [HttpPost("enderecos/principal/{id}")]
    [Authorize]
    public async Task<IActionResult> DefinirEnderecoParincipal(Guid id)
    {
        var usuarioId = userManager.GetUserId(User)!;
        await enderecoService.DefinirPrincipalAsync(id, usuarioId);
        return RedirectToAction(nameof(Perfil));
    }

    [HttpGet("acesso-negado")]
    public IActionResult AcessoNegado() => View();
}
