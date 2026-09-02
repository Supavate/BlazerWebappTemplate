using System.Security.Claims;

namespace MyWebApp.Application.Abstractions.Authentication;

public interface ICurrentUserService
{
    Task<ClaimsPrincipal> GetUserAsync();
}
