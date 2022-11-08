using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models;

public class ExtendedItem
{
    public string Title { get; set; }
    public string Description { get; set; }
    public FluentSymbols Symbol { get; set; }
}
