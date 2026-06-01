using Glamour.Application;
using Glamour.Infrastructure;
using Glamour.Infrastructure.Data;
using Glamour.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/glamour-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    builder.Services.AddControllersWithViews();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.ConfigureApplicationCookie(opt =>
    {
        opt.LoginPath = "/conta/login";
        opt.LogoutPath = "/conta/logout";
        opt.AccessDeniedPath = "/conta/acesso-negado";
        opt.Cookie.HttpOnly = true;
        opt.Cookie.SameSite = SameSiteMode.Strict;
        opt.ExpireTimeSpan = TimeSpan.FromDays(7);
        opt.SlidingExpiration = true;
    });

    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(opt =>
    {
        opt.IdleTimeout = TimeSpan.FromDays(7);
        opt.Cookie.HttpOnly = true;
        opt.Cookie.IsEssential = true;
    });

    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    // Migrations e seed automáticos
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<GlamourDbContext>();
        await db.Database.MigrateAsync();

        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedAsync(userMgr, roleMgr);
        await Glamour.Infrastructure.Data.DataSeeder.SeedDadosDemoAsync(db);
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/erro");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseSession();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute("areas", "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
    app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação terminou inesperadamente.");
}
finally
{
    Log.CloseAndFlush();
}

static async Task SeedAsync(UserManager<ApplicationUser> userMgr, RoleManager<IdentityRole> roleMgr)
{
    foreach (var role in new[] { "Admin", "Cliente" })
        if (!await roleMgr.RoleExistsAsync(role))
            await roleMgr.CreateAsync(new IdentityRole(role));

    if (await userMgr.FindByEmailAsync("admin@glamour.com") == null)
    {
        var admin = new ApplicationUser
        {
            UserName = "admin@glamour.com",
            Email = "admin@glamour.com",
            Nome = "Administrador",
            EmailConfirmed = true,
            Ativo = true
        };
        await userMgr.CreateAsync(admin, "Admin@123456");
        await userMgr.AddToRoleAsync(admin, "Admin");
    }
}
