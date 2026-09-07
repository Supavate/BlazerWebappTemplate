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
            new SalesPeriodNavEntry("january", "Sales January", Icons.Material.Filled.CalendarMonth),
            new SalesPeriodNavEntry("february", "Sales February", Icons.Material.Filled.CalendarMonth));

        var factory = new SubmenuProviderFactory(
            new FakeServiceProvider((typeof(FakeSubmenuProvider), provider)),
            options);

        var merger = new NavigationMerger(navigation, factory, options);

        var items = await merger.GetItemsAsync();

        var reportsNode = Assert.Single(items, item => item.Route == "/reports");
        Assert.Collection(
            reportsNode.Children,
            january =>
            {
                Assert.Equal("Sales January", january.Title);
                Assert.Equal("/reports/january", january.Route);
            },
            february =>
            {
                Assert.Equal("Sales February", february.Title);
                Assert.Equal("/reports/february", february.Route);
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

    [Fact]
    public async Task AppendsGeneratedChildrenAfterStaticChildren()
    {
        var navigation = new NavigationService(typeof(Program).Assembly);
        var options = CreateOptions(("SettingsData", typeof(FakeSubmenuProvider), "/settings"));
        var provider = new FakeSubmenuProvider(
            new TestSubmenuEntry("audit", "Audit", "history"));
        var merger = CreateMerger(navigation, options, (typeof(FakeSubmenuProvider), provider));

        var items = await merger.GetItemsAsync();

        var settings = Assert.Single(items, item => item.Route == "/settings");
        Assert.Collection(
            settings.Children,
            users => Assert.Equal("/settings/users", users.Route),
            audit => Assert.Equal("/settings/audit", audit.Route));
    }

    [Fact]
    public async Task PreservesCatalogAndProviderSequenceForSameParent()
    {
        var navigation = new NavigationService(typeof(Program).Assembly);
        var options = CreateOptions(
            ("First", typeof(FirstSubmenuProvider), "/reports"),
            ("Second", typeof(SecondSubmenuProvider), "/reports"));
        var first = new FirstSubmenuProvider(
            new TestSubmenuEntry("zulu", "Zulu", "circle"),
            new TestSubmenuEntry("alpha", "Alpha", "circle"));
        var second = new SecondSubmenuProvider(
            new TestSubmenuEntry("second", "Second", "circle"));
        var merger = CreateMerger(
            navigation,
            options,
            (typeof(FirstSubmenuProvider), first),
            (typeof(SecondSubmenuProvider), second));

        var items = await merger.GetItemsAsync();

        var reports = Assert.Single(items, item => item.Route == "/reports");
        Assert.Equal(
            ["/reports/zulu", "/reports/alpha", "/reports/second"],
            reports.Children.Select(item => item.Route));
    }

    [Theory]
    [InlineData("", "Valid", "blank key")]
    [InlineData("valid", " ", "blank title")]
    public async Task RejectsBlankGeneratedEntryFields(
        string key,
        string title,
        string expectedMessage)
    {
        var navigation = new NavigationService(typeof(Program).Assembly);
        var options = CreateOptions(("Invalid", typeof(FakeSubmenuProvider), "/reports"));
        var provider = new FakeSubmenuProvider(new TestSubmenuEntry(key, title, "circle"));
        var merger = CreateMerger(navigation, options, (typeof(FakeSubmenuProvider), provider));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => merger.GetItemsAsync());

        Assert.Contains(expectedMessage, exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RejectsDuplicateGeneratedKeysIgnoringCaseAndWhitespace()
    {
        var navigation = new NavigationService(typeof(Program).Assembly);
        var options = CreateOptions(("Duplicates", typeof(FakeSubmenuProvider), "/reports"));
        var provider = new FakeSubmenuProvider(
            new TestSubmenuEntry("january", "January", "circle"),
            new TestSubmenuEntry(" JANUARY ", "January duplicate", "circle"));
        var merger = CreateMerger(navigation, options, (typeof(FakeSubmenuProvider), provider));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => merger.GetItemsAsync());

        Assert.Contains("duplicate key", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static IOptions<SubmenuCatalogsOptions> CreateOptions(
        params (string key, Type providerType, string parentRoute)[] catalogs)
    {
        var entries = new Dictionary<string, SubmenuCatalogEntry>();
        foreach (var catalog in catalogs)
        {
            entries.Add(catalog.key, new SubmenuCatalogEntry
            {
                ProviderType = catalog.providerType.AssemblyQualifiedName!,
                ParentRoute = catalog.parentRoute,
                Icon = "circle"
            });
        }

        return Options.Create(new SubmenuCatalogsOptions { Catalogs = entries });
    }

    private static NavigationMerger CreateMerger(
        NavigationService navigation,
        IOptions<SubmenuCatalogsOptions> options,
        params (Type type, object instance)[] services)
    {
        var factory = new SubmenuProviderFactory(new FakeServiceProvider(services), options);
        return new NavigationMerger(navigation, factory, options);
    }

    private sealed class FakeSubmenuProvider(
        params ISubmenuEntry[] entries) : ISubmenuProvider
    {
        public Task<IReadOnlyList<ISubmenuEntry>> GetItemsAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ISubmenuEntry>>(entries);
    }

    private sealed class FirstSubmenuProvider(
        params ISubmenuEntry[] entries) : ISubmenuProvider
    {
        public Task<IReadOnlyList<ISubmenuEntry>> GetItemsAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ISubmenuEntry>>(entries);
    }

    private sealed class SecondSubmenuProvider(
        params ISubmenuEntry[] entries) : ISubmenuProvider
    {
        public Task<IReadOnlyList<ISubmenuEntry>> GetItemsAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ISubmenuEntry>>(entries);
    }

    private sealed record TestSubmenuEntry(
        string Key,
        string Title,
        string Icon) : ISubmenuEntry;

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
