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
    PedidoService pedidoService) : Controller
{
    [HttpGet("registrar")]
    public IActionResult Registrar() => View();

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
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
    {
        var resultado = await signInManager.PasswordSignInAsync(dto.Email, dto.Senha, dto.LembrarMe, lockoutOnFailure: true);
        if (resultado.Succeeded)
            return LocalRedirect(returnUrl ?? "/");

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

    [HttpGet("meus-pedidos")]
    [Authorize]
    public async Task<IActionResult> MeusPedidos()
    {
        var usuarioId = userManager.GetUserId(User)!;
        var pedidos = await pedidoService.ObterPorUsuarioAsync(usuarioId);
        return View(pedidos);
    }

    [HttpGet("acesso-negado")]
    public IActionResult AcessoNegado() => View();
}
