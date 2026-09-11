using MyWebApp.Infrastructure.Reports;

namespace MyWebApp.UnitTests.Infrastructure;

public sealed class MockReportDataSourceTests
{
    [Fact]
    public void ProvidesTwelveOrderedPeriods()
    {
        Assert.Collection(
            MockReportDataSource.Periods,
            january => Assert.Equal("Sales January", january.Title),
            february => Assert.Equal("Sales February", february.Title),
            march => Assert.Equal("Sales March", march.Title),
            april => Assert.Equal("Sales April", april.Title),
            may => Assert.Equal("Sales May", may.Title),
            june => Assert.Equal("Sales June", june.Title),
            july => Assert.Equal("Sales July", july.Title),
            august => Assert.Equal("Sales August", august.Title),
            september => Assert.Equal("Sales September", september.Title),
            october => Assert.Equal("Sales October", october.Title),
            november => Assert.Equal("Sales November", november.Title),
            december => Assert.Equal("Sales December", december.Title));
    }

    [Fact]
    public void FindMatchesPeriodKeyIgnoringCase()
    {
        var report = MockReportDataSource.Find("FEBRUARY");

        Assert.NotNull(report);
        Assert.Equal("february", report.Key);
        Assert.Equal("Sales February", report.Title);
    }

    [Fact]
    public void FindReturnsNullForUnknownPeriodKey()
    {
        Assert.Null(MockReportDataSource.Find("unknown-period"));
    }
}