using MyWebApp.Application.Abstractions.Reports;
using MyWebApp.Application.Models.Reports;

namespace MyWebApp.Infrastructure.Reports;

/// <summary>
/// Direct implementation of the report contracts that reads from
/// <see cref="MockReportDataSource"/>. Replace this with a repository that
/// queries a database when a real data source is available.
/// </summary>
public sealed class ReportMockService : ISalesPeriodCatalog, ISalesReportService
{
    public Task<IReadOnlyList<SalesPeriodNavEntry>> GetPeriodsAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(MockReportDataSource.Periods);
    }

    public Task<SalesPeriodReport?> GetReportAsync(
        string periodKey,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(MockReportDataSource.Find(periodKey));
    }
}
