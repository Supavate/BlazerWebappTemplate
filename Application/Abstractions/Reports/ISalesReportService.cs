using MyWebApp.Application.Models.Reports;

namespace MyWebApp.Application.Abstractions.Reports;

public interface ISalesReportService
{
    Task<SalesPeriodReport?> GetReportAsync(
        string periodKey,
        CancellationToken cancellationToken = default);
}