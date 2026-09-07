using MyWebApp.Application.Models.Reports;

namespace MyWebApp.Application.Abstractions.Reports;

public interface ISubmenuProvider
{
    /// <summary>
    /// Returns submenu entries in their intended display order.
    /// </summary>
    Task<IReadOnlyList<ISubmenuEntry>> GetItemsAsync(
        CancellationToken cancellationToken = default);
}
