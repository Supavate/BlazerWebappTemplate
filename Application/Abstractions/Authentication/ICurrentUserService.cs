using MyWebApp.Application.Models.Authentication;

namespace MyWebApp.Application.Abstractions.Authentication;

public interface ICurrentUserService
{
    Task<CurrentUser?> GetCurrentUserAsync(
        CancellationToken cancellationToken = default);
}
