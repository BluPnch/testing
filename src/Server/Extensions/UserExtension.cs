using System.Security.Claims;


namespace Server.Extensions;

static public class UserExtension
{
    static public Guid GetUserIdFromToken(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("sub");
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
        {
            return userId;
        }
        return Guid.Empty;
    }
}