using System.Security.Claims;
using Common.Web.Errors.Exceptions;

namespace Common.Web.Auth;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var idString = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        if (!int.TryParse(idString, out var id))
        {
            throw new UnauthorizedException();
        }

        return id;
    }
}
