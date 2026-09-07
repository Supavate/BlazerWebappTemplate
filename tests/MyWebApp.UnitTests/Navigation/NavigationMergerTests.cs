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
        var options = Options.Create(new SubmenuCatalogsOptions
        {
            Catalogs = new Dictionary<string, SubmenuCatalogEntry>
            {
                ["SalesReports"] = new SubmenuCatalogEntry
                {
                    ProviderType = typeof(FakeSubmenuProvider).AssemblyQualifiedName!,
                    ParentRoute = "/reports",
                    Icon = "calendar_month"
                }
            }
        });

        var provider = new FakeSubmenuProvider(
            new SalesPeriodNavEntry("january", "Sales January", Icons.Material.Filled.CalendarMonth, 2),
            new SalesPeriodNavEntry("february", "Sales February", Icons.Material.Filled.CalendarMonth, 1));

        var factory = new SubmenuProviderFactory(
            new FakeServiceProvider((typeof(FakeSubmenuProvider), provider)),
            options);

        var merger = new NavigationMerger(navigation, factory, options);

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
        var options = Options.Create(new SubmenuCatalogsOptions
        {
            Catalogs = new Dictionary<string, SubmenuCatalogEntry>
            {
                ["SalesReports"] = new SubmenuCatalogEntry
                {
                    ProviderType = typeof(FakeSubmenuProvider).AssemblyQualifiedName!,
                    ParentRoute = "/reports",
                    Icon = "calendar_month"
                }
            }
        });

        var provider = new FakeSubmenuProvider();
        var factory = new SubmenuProviderFactory(
            new FakeServiceProvider((typeof(FakeSubmenuProvider), provider)),
            options);
        var merger = new NavigationMerger(navigation, factory, options);

        var items = await merger.GetItemsAsync();

        var reportsNode = Assert.Single(items, item => item.Route == "/reports");
        Assert.Empty(reportsNode.Children);
    }

    private sealed class FakeSubmenuProvider(
        params ISubmenuEntry[] entries) : ISubmenuProvider
    {
        public Task<IReadOnlyList<ISubmenuEntry>> GetItemsAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ISubmenuEntry>>(entries);
    }

    private sealed class FakeServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services;

        public FakeServiceProvider(params (Type type, object instance)[] services)
        {
            _services = services.ToDictionary(service => service.type, service => service.instance);
        }

        public object? GetService(Type serviceType) =>
            _services.TryGetValue(serviceType, out var service) ? service : null;
    }
}
