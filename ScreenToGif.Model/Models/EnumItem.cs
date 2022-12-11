namespace ScreenToGif.Domain.Models;

public class EnumItem<T> where T : Enum
{
    public T Type { get; set; }

    public string NameKey { get; set; }

    public string Name { get; set; }

    public string Parameter { get; set; }

    public EnumItem()
    { }

    public EnumItem(T type, string nameKey, string name, string parameter)
    {
        Type = type;
        NameKey = nameKey;
        Name = name;
        Parameter = parameter;
    }

    public EnumItem(T type, string nameKey, string parameter)
    {
        Type = type;
        NameKey = nameKey;
        Parameter = parameter;
    }
}