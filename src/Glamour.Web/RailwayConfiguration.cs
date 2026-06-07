namespace Glamour.Web;

public static class RailwayConfiguration
{
    public static void AddRailway(this WebApplicationBuilder builder)
    {
        var port = Environment.GetEnvironmentVariable("PORT");
        if (!string.IsNullOrWhiteSpace(port))
            builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

        var overrides = new Dictionary<string, string?>();

        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(databaseUrl))
            overrides["ConnectionStrings:DefaultConnection"] = ConverterPostgres(databaseUrl);

        var redisUrl = Environment.GetEnvironmentVariable("REDIS_URL");
        if (!string.IsNullOrWhiteSpace(redisUrl))
            overrides["ConnectionStrings:Redis"] = ConverterRedis(redisUrl);

        if (overrides.Count > 0)
            builder.Configuration.AddInMemoryCollection(overrides);
    }

    private static string ConverterPostgres(string url)
    {
        var uri = new Uri(url);
        var credenciais = uri.UserInfo.Split(':', 2);
        var usuario = Uri.UnescapeDataString(credenciais[0]);
        var senha = Uri.UnescapeDataString(credenciais.Length > 1 ? credenciais[1] : "");
        var banco = uri.AbsolutePath.TrimStart('/');
        var porta = uri.Port > 0 ? uri.Port : 5432;

        return $"Host={uri.Host};Port={porta};Database={banco};Username={usuario};Password={senha};" +
               "SSL Mode=Prefer;Trust Server Certificate=true";
    }

    private static string ConverterRedis(string url)
    {
        var uri = new Uri(url);
        var porta = uri.Port > 0 ? uri.Port : 6379;
        var conexao = $"{uri.Host}:{porta}";

        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            var credenciais = uri.UserInfo.Split(':', 2);
            var senha = credenciais.Length > 1 ? credenciais[1] : credenciais[0];
            conexao += $",password={Uri.UnescapeDataString(senha)}";
        }

        if (uri.Scheme == "rediss")
            conexao += ",ssl=true";

        return conexao;
    }
}
