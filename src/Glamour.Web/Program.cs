using System.IO.Compression;
using System.Threading.RateLimiting;
using Glamour.Application;
using Glamour.Infrastructure;
using Glamour.Infrastructure.Data;
using Glamour.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
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

    // MVC
    builder.Services.AddControllersWithViews();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Identity cookie
    builder.Services.ConfigureApplicationCookie(opt =>
    {
        opt.LoginPath = "/conta/login";
        opt.LogoutPath = "/conta/logout";
        opt.AccessDeniedPath = "/conta/acesso-negado";
        opt.Cookie.HttpOnly = true;
        opt.Cookie.SameSite = SameSiteMode.Strict;
        opt.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        opt.ExpireTimeSpan = TimeSpan.FromDays(7);
        opt.SlidingExpiration = true;
    });

    // Sessão
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(opt =>
    {
        opt.IdleTimeout = TimeSpan.FromDays(7);
        opt.Cookie.HttpOnly = true;
        opt.Cookie.IsEssential = true;
        opt.Cookie.SameSite = SameSiteMode.Strict;
    });

    // Output cache
    builder.Services.AddOutputCache(opt =>
    {
        opt.AddBasePolicy(b => b.Expire(TimeSpan.FromMinutes(2)));
        opt.AddPolicy("home", b => b.Expire(TimeSpan.FromMinutes(5)).Tag("home"));
        opt.AddPolicy("catalogo", b => b.Expire(TimeSpan.FromMinutes(2)).Tag("catalogo"));
        opt.AddPolicy("produto", b => b.Expire(TimeSpan.FromMinutes(10)).Tag("produto"));
    });

    // Compressão Brotli/Gzip
    builder.Services.AddResponseCompression(opt =>
    {
        opt.EnableForHttps = true;
        opt.Providers.Add<BrotliCompressionProvider>();
        opt.Providers.Add<GzipCompressionProvider>();
    });
    builder.Services.Configure<BrotliCompressionProviderOptions>(opt => opt.Level = CompressionLevel.Fastest);
    builder.Services.Configure<GzipCompressionProviderOptions>(opt => opt.Level = CompressionLevel.Fastest);

    // Rate limiting
    builder.Services.AddRateLimiter(opt =>
    {
        opt.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        opt.AddFixedWindowLimiter("login", o =>
        {
            o.Window = TimeSpan.FromMinutes(5);
            o.PermitLimit = 10;
            o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            o.QueueLimit = 0;
        });

        opt.AddFixedWindowLimiter("api", o =>
        {
            o.Window = TimeSpan.FromMinutes(1);
            o.PermitLimit = 60;
            o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            o.QueueLimit = 0;
        });
    });

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<Glamour.Web.Services.ImagemService>();

    var app = builder.Build();

    // Migrations e seed automáticos
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<GlamourDbContext>();
        await db.Database.MigrateAsync();

        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedAsync(userMgr, roleMgr);
        await DataSeeder.SeedDadosDemoAsync(db);
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/erro");
        app.UseHsts();
    }

    app.UseResponseCompression();
    app.UseStatusCodePagesWithReExecute("/erro/{0}");
    app.UseHttpsRedirection();

    // Headers de segurança
    app.Use(async (ctx, next) =>
    {
        ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        ctx.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
        ctx.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        ctx.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        ctx.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
        await next();
    });

    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=2592000,immutable");
        }
    });

    app.UseRouting();
    app.UseRateLimiter();
    app.UseOutputCache();
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
