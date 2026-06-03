using System.Security.Claims;

namespace Glamour.Web;

public static class CarrinhoHttpExtensions
{
    public static string ObterCarrinhoId(this HttpContext ctx)
    {
        var userId = ctx.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
            return "u:" + userId;

        var id = ctx.Session.GetString("CarrinhoId");
        if (string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString();
            ctx.Session.SetString("CarrinhoId", id);
        }
        return id;
    }
}
