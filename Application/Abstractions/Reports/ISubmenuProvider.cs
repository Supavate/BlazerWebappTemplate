using MyWebApp.Application.Models.Reports;

namespace MyWebApp.Application.Abstractions.Reports;

public interface ISubmenuProvider
{
    Task<IReadOnlyList<ISubmenuEntry>> GetItemsAsync(
        CancellationToken cancellationToken = default);
}
