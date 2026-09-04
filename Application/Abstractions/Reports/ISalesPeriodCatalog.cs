using MyWebApp.Application.Models.Reports;

namespace MyWebApp.Application.Abstractions.Reports;

public interface ISalesPeriodCatalog
{
    Task<IReadOnlyList<SalesPeriodNavEntry>> GetPeriodsAsync(
        CancellationToken cancellationToken = default);
}