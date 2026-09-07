namespace MyWebApp.Application.Models.Reports;

public interface ISubmenuEntry
{
    string Key { get; }
    string Title { get; }
    string Icon { get; }
    int Order { get; }
}
