

using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public class CookieGenerator
{
    public static CookieOptions GenerateCookie(DateTimeOffset expiryDate)
    {
        var cookieOptions = new CookieOptions()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Expires = expiryDate
        };
        return cookieOptions;
    }
}
