using System.IO.Compression;
using System.Threading.RateLimiting;
using Glamour.Application;
using Glamour.Infrastructure;
using Glamour.Infrastructure.Data;
using Glamour.Infrastructure.Identity;
using Glamour.Web;
using Microsoft.AspNetCore.HttpOverrides;
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
    builder.AddRailway();

    builder.Services.Configure<ForwardedHeadersOptions>(opt =>
    {
        opt.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        opt.KnownIPNetworks.Clear();
        opt.KnownProxies.Clear();
    });

    builder.Services.AddControllersWithViews(opt =>
    {
        opt.ModelBinderProviders.Insert(0, new Glamour.Web.ModelBinding.InvariantDecimalModelBinderProvider());
    });
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.ConfigureApplicationCookie(opt =>
    {
        opt.LoginPath = "/conta/login";
        opt.LogoutPath = "/conta/logout";
        opt.AccessDeniedPath = "/conta/acesso-negado";
        opt.Cookie.HttpOnly = true;
        opt.Cookie.SameSite = SameSiteMode.Lax;
        opt.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        opt.ExpireTimeSpan = TimeSpan.FromDays(7);
        opt.SlidingExpiration = true;
    });

    var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
    var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
    {
        builder.Services.AddAuthentication().AddGoogle(opt =>
        {
            opt.ClientId = googleClientId;
            opt.ClientSecret = googleClientSecret;
            opt.CallbackPath = "/signin-google";
        });
    }

    var redisConn = builder.Configuration.GetConnectionString("Redis");
    if (!string.IsNullOrWhiteSpace(redisConn))
        builder.Services.AddStackExchangeRedisCache(opt =>
        {
            var options = StackExchange.Redis.ConfigurationOptions.Parse(redisConn);
            options.AbortOnConnectFail = false;
            opt.ConfigurationOptions = options;
        });
    else
        builder.Services.AddDistributedMemoryCache();

    builder.Services.AddSession(opt =>
    {
        opt.IdleTimeout = TimeSpan.FromDays(7);
        opt.Cookie.HttpOnly = true;
        opt.Cookie.IsEssential = true;
        opt.Cookie.SameSite = SameSiteMode.Strict;
    });

    builder.Services.AddOutputCache(opt =>
    {
        opt.AddPolicy("home", b => b.Expire(TimeSpan.FromMinutes(5)).Tag("home"));
        opt.AddPolicy("catalogo", b => b.Expire(TimeSpan.FromMinutes(2)).Tag("catalogo"));
        opt.AddPolicy("produto", b => b.Expire(TimeSpan.FromMinutes(10)).Tag("produto"));
    });

    builder.Services.AddResponseCompression(opt =>
    {
        opt.EnableForHttps = true;
        opt.Providers.Add<BrotliCompressionProvider>();
        opt.Providers.Add<GzipCompressionProvider>();
    });
    builder.Services.Configure<BrotliCompressionProviderOptions>(opt => opt.Level = CompressionLevel.Fastest);
    builder.Services.Configure<GzipCompressionProviderOptions>(opt => opt.Level = CompressionLevel.Fastest);

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

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<GlamourDbContext>();
        await db.Database.MigrateAsync();

        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedAsync(userMgr, roleMgr);
        await DataSeeder.SeedDadosDemoAsync(db);
    }

    app.UseForwardedHeaders();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/erro");
        app.UseHsts();
    }

    app.UseResponseCompression();
    app.UseStatusCodePagesWithReExecute("/erro/{0}");
    app.UseHttpsRedirection();

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

    app.Use(async (context, next) =>
    {
        var usuario = context.User;
        if (usuario.Identity?.IsAuthenticated == true
            && usuario.IsInRole(Glamour.Domain.Enums.RolesUsuario.Admin)
            && usuario.FindFirst(Glamour.Web.Controllers.ContaController.ClaimMetodoLogin)?.Value
               != Glamour.Web.Controllers.ContaController.MetodoGoogle)
        {
            var path = context.Request.Path.Value ?? "";
            var liberado = path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/conta", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/erro", StringComparison.OrdinalIgnoreCase);
            if (!liberado)
            {
                context.Response.Redirect("/Admin");
                return;
            }
        }
        await next();
    });

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
