using MudBlazor;
using MyWebApp.Application.Models.Reports;

namespace MyWebApp.Infrastructure.Reports;

/// <summary>
/// In-memory data used by the mock report services. Replace it with data from
/// the production report API without changing page or navigation code.
/// </summary>
public static class MockReportDataSource
{
    public static IReadOnlyList<SalesPeriodNavEntry> Periods { get; } =
    [
        new("january", "Sales January", Icons.Material.Filled.CalendarMonth),
        new("february", "Sales February", Icons.Material.Filled.CalendarMonth),
        new("march", "Sales March", Icons.Material.Filled.CalendarMonth),
        new("april", "Sales April", Icons.Material.Filled.CalendarMonth),
        new("may", "Sales May", Icons.Material.Filled.CalendarMonth),
        new("june", "Sales June", Icons.Material.Filled.CalendarMonth),
        new("july", "Sales July", Icons.Material.Filled.CalendarMonth),
        new("august", "Sales August", Icons.Material.Filled.CalendarMonth),
        new("september", "Sales September", Icons.Material.Filled.CalendarMonth),
        new("october", "Sales October", Icons.Material.Filled.CalendarMonth),
        new("november", "Sales November", Icons.Material.Filled.CalendarMonth),
        new("december", "Sales December", Icons.Material.Filled.CalendarMonth)
    ];

    public static SalesPeriodReport? Find(string periodKey) =>
        Periods
            .Where(period => string.Equals(period.Key, periodKey, StringComparison.OrdinalIgnoreCase))
            .Select(period => new SalesPeriodReport(period.Key, period.Title))
            .FirstOrDefault();
}
