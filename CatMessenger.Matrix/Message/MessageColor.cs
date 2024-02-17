namespace CatMessenger.Matrix.Message;

public class MessageColor
{
    public static readonly MessageColor Black = new("black");
    public static readonly MessageColor DarkBlue = new("dark_blue");
    public static readonly MessageColor DarkGreen = new("dark_green");
    public static readonly MessageColor DarkAqua = new("dark_aqua");
    public static readonly MessageColor DarkRed = new("dark_red");
    public static readonly MessageColor DarkPurple = new("dark_purple");
    public static readonly MessageColor Gold = new("gold");
    public static readonly MessageColor Gray = new("gray");
    public static readonly MessageColor DarkGray = new("dark_gray");
    public static readonly MessageColor Blue = new("blue");
    public static readonly MessageColor Green = new("green");
    public static readonly MessageColor Aqua = new("aqua");
    public static readonly MessageColor Red = new("red");
    public static readonly MessageColor LightPurple = new("light_purple");
    public static readonly MessageColor Yellow = new("yellow");
    public static readonly MessageColor White = new("white");
    public static readonly MessageColor Reset = new("reset");

    protected MessageColor(string name)
    {
        Name = name;
    }

    public MessageColor(int hex)
    {
        Hex = hex;
    }

    private string? Name { get; }
    private int? Hex { get; }

    public bool IsHex()
    {
        return Hex is not null;
    }

    public string AsString()
    {
        return IsHex() ? $"#{Hex}" : Name!;
    }

    public static MessageColor FromString(string str)
    {
        return str switch
        {
            "black" => Black,
            "dark_blue" => DarkBlue,
            "dark_green" => DarkGreen,
            "dark_aqua" => DarkAqua,
            "dark_red" => DarkRed,
            "dark_purple" => DarkPurple,
            "gold" => Gold,
            "gray" => Gray,
            "dark_gray" => DarkGray,
            "blue" => Blue,
            "green" => Green,
            "aqua" => Aqua,
            "red" => Red,
            "light_purple" => LightPurple,
            "yellow" => Yellow,
            "white" => White,
            "reset" => Reset,
            _ => str.StartsWith('#') ? new MessageColor(int.Parse(str.Remove(0, 1))) : Reset
        };
    }
}