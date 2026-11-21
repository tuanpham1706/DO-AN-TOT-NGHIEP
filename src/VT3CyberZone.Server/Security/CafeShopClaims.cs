using System.Security.Claims;

namespace VT3CyberZone.Server.Security;

public static class CafeShopClaims
{
    public const string ShopId = "cafe_shop_id";

    public static int GetCafeShopId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ShopId);
        if (!int.TryParse(value, out var shopId) || shopId <= 0)
            throw new InvalidOperationException("The authenticated shop account has no valid shop scope.");

        return shopId;
    }
}
