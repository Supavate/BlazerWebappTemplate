using Microsoft.Extensions.Options;
using MudBlazor;
using MyWebApp.Application.Abstractions.Reports;
using MyWebApp.Application.Models.Reports;
using MyWebApp.Configurations;
using MyWebApp.Navigation;

namespace MyWebApp.UnitTests.Navigation;

public sealed class NavigationMergerTests
{
    [Fact]
    public async Task AppendsDataDrivenPeriodsUnderConfiguredParent()
    {
        var navigation = new NavigationService(typeof(Program).Assembly);
        var catalog = new FakePeriodCatalog(
        [
            new SalesPeriodNavEntry("january", "Sales January", Icons.Material.Filled.CalendarMonth, 2),
            new SalesPeriodNavEntry("february", "Sales February", Icons.Material.Filled.CalendarMonth, 1)
        ]);
        var options = Options.Create(new ReportApiOptions
        {
            ParentRoute = "/reports"
        });

        var merger = new NavigationMerger(navigation, catalog, options);

        var items = await merger.GetItemsAsync();

        var reportsNode = Assert.Single(items, item => item.Route == "/reports");
        Assert.Collection(
            reportsNode.Children,
            february =>
            {
                Assert.Equal("Sales February", february.Title);
                Assert.Equal("/reports/february", february.Route);
            },
            january =>
            {
                Assert.Equal("Sales January", january.Title);
                Assert.Equal("/reports/january", january.Route);
            });
    }

    [Fact]
    public async Task KeepsStaticTreeWhenCatalogReturnsNoPeriods()
    {
        var navigation = new NavigationService(typeof(Program).Assembly);
        var catalog = new FakePeriodCatalog([]);
        var merger = new NavigationMerger(
            navigation,
            catalog,
            Options.Create(new ReportApiOptions()));

        var items = await merger.GetItemsAsync();

        var reportsNode = Assert.Single(items, item => item.Route == "/reports");
        Assert.Empty(reportsNode.Children);
    }

    private sealed class FakePeriodCatalog(
        IReadOnlyList<SalesPeriodNavEntry> periods) : ISalesPeriodCatalog
    {
        public Task<IReadOnlyList<SalesPeriodNavEntry>> GetPeriodsAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(periods);
    }
}