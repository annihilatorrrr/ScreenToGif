using System.Windows.Media;

namespace ScreenToGif.Domain.Models;

public class ExtendedItem
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DrawingBrush Drawing { get; set; }
}
