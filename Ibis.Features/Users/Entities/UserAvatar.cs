using System.Drawing;
using System.Globalization;

namespace Ibis.Features.Users;

public class UserAvatar
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Initials => string.IsNullOrWhiteSpace(Name) ? "" : string.Join(string.Empty, Name.Split(' ').Select(x => x[0]));
    public bool IsOnline { get; set; }
    public string ForegroundColor { get; set; }
    public string BackgroundColor { get; set; }
    public string? Language { get; set; }
    public string? Emoji { get; set; }
    public string? SkinTone { get; set; }
    public string? Pronouns { get; set; }
    public string? Description { get; set; }

    public UserAvatar() : this("", "")
    {
    }


    public UserAvatar(string id, string name)
    {
        Id = id; 
        Name = name;
        ForegroundColor = ForegroundColors().OrderBy(x => Guid.NewGuid()).First();
        BackgroundColor = CalculateBackgroundColor(ForegroundColor);
    }

    public UserAvatar(string id, string name, string language, string foregroundColor, string backgroundColor, string skinTone, string emoji)
        : this(id, name)
    {
        Language = language;
        ForegroundColor = foregroundColor;
        BackgroundColor = backgroundColor;
        SkinTone = skinTone;
        Emoji = emoji;
    }

    public static string CalculateBackgroundColor(string foregroundColor)
    {
        // derived from https://stackoverflow.com/a/1626175

        var color = ColorTranslator.FromHtml(foregroundColor);
        int max = Math.Max(color.R, Math.Max(color.G, color.B));
        int min = Math.Min(color.R, Math.Min(color.G, color.B));

        var hue = color.GetHue();
        var saturation = (max == 0) ? 0 : 1d - (1d * min / max);
        var value = max / 255d;

        var background = ColorFromHSV(hue, saturation * 0.7, value + ((1 - value) * 0.8));
        return ColorTranslator.ToHtml(background);
    }

    public static Color ColorFromHSV(double hue, double saturation, double value)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        double f = hue / 60 - Math.Floor(hue / 60);

        value *= 255;
        int v = Convert.ToInt32(value);
        int p = Convert.ToInt32(value * (1 - saturation));
        int q = Convert.ToInt32(value * (1 - f * saturation));
        int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

        if (hi == 0)
            return Color.FromArgb(255, v, t, p);
        else if (hi == 1)
            return Color.FromArgb(255, q, v, p);
        else if (hi == 2)
            return Color.FromArgb(255, p, v, t);
        else if (hi == 3)
            return Color.FromArgb(255, p, q, v);
        else if (hi == 4)
            return Color.FromArgb(255, t, p, v);
        else
            return Color.FromArgb(255, v, p, q);
    }

    public static List<string> ForegroundColors() => new()
    {
        // generated from http://phrogz.net/css/distinct-colors.html 
        // hue 29-330, sat 100-23, value 70-20, 50 colors
        
        "#b25600", "#a69c53", "#435954", "#5976b3", "#a62993", "#331f0d", "#414d00", 
        "#00b39e", "#434a59", "#594356", "#b28459", "#2c331a", "#004b4d", "#000e4d", 
        "#990059", "#b26e00", "#61a600", "#53a4a6", "#1d2173", "#33001e", "#593700", 
        "#7e8c69", "#0080b3", "#5a53a6", "#59163d", "#403a30", "#347300", "#00374d", 
        "#2f264d", "#a67c94", "#735600", "#53a665", "#396273", "#857ca6", "#332600", 
        "#004016", "#001733", "#5529a6", "#665933", "#86b39b", "#1a3c66", "#380040", 
        "#a69200", "#1a6653", "#869ab3", "#5d1a66"
    };

    public static List<string> SkinTones()
    {
        // derived from https://blog.mzikmund.com/2017/01/working-with-emoji-skin-tones-in-apps/

        List<string> tones = new()
        {
            "1F3FB", "1F3FC", "1F3FD", "1F3FE", "1F3FF"
        };
        return tones
            .Select(x => char.ConvertFromUtf32(int.Parse(x, NumberStyles.HexNumber)))
            .ToList();
    }

    public static List<string> Emojis() => new()
    {
        // all skin-tone-compatible emojis
        // derived from CSV at https://github.com/Remotionco/Emoji-Library-and-Utilities
        
        "👶", "🧒", "👦", "👧", "🧑", "👱", "👨", "🧔", "🧔‍♂️", "🧔‍♀️", "👨‍🦰",
        "👨‍🦱", "👨‍🦳", "👨‍🦲", "👩", "👩‍🦰", "🧑‍🦰", "👩‍🦱", "🧑‍🦱", "👩‍🦳", "🧑‍🦳",
        "👩‍🦲", "🧑‍🦲", "👱‍♀️", "👱‍♂️", "🧓", "👴", "👵", "🙍", "🙍‍♂️", "🙍‍♀️", "🙎", "🙎‍♂️",
        "🙎‍♀️", "🙅", "🙅‍♂️", "🙅‍♀️", "🙆", "🙆‍♂️", "🙆‍♀️", "💁", "💁‍♂️", "💁‍♀️", "🙋", "🙋‍♂️",
        "🙋‍♀️", "🧏", "🧏‍♂️", "🧏‍♀️", "🙇", "🙇‍♂️", "🙇‍♀️", "🤦", "🤦‍♂️", "🤦‍♀️", "🤷", "🤷‍♂️",
        "🤷‍♀️", "🧑‍⚕️", "👨‍⚕️", "👩‍⚕️", "🧑‍🎓", "👨‍🎓", "👩‍🎓", "🧑‍🏫", "👨‍🏫", "👩‍🏫",
        "🧑‍⚖️", "👨‍⚖️", "👩‍⚖️", "🧑‍🌾", "👨‍🌾", "👩‍🌾", "🧑‍🍳", "👨‍🍳", "👩‍🍳", "🧑‍🔧",
        "👨‍🔧", "👩‍🔧", "🧑‍🏭", "👨‍🏭", "👩‍🏭", "🧑‍💼", "👨‍💼", "👩‍💼", "🧑‍🔬", "👨‍🔬",
        "👩‍🔬", "🧑‍💻", "👨‍💻", "👩‍💻", "🧑‍🎤", "👨‍🎤", "👩‍🎤", "🧑‍🎨", "👨‍🎨", "👩‍🎨",
        "🧑‍✈️", "👨‍✈️", "👩‍✈️", "🧑‍🚀", "👨‍🚀", "👩‍🚀", "🧑‍🚒", "👨‍🚒", "👩‍🚒", "👮", "👮‍♂️",
        "👮‍♀️", "🕵️", "🕵️‍♂️", "🕵️‍♀️", "💂", "💂‍♂️", "💂‍♀️", "🥷", "👷", "👷‍♂️", "👷‍♀️", "🤴", "👸",
        "👳", "👳‍♂️", "👳‍♀️", "👲", "🧕", "🤵", "🤵‍♂️", "🤵‍♀️", "👰", "👰‍♂️", "👰‍♀️", "🤰", "🤱", "👩‍🍼",
        "👨‍🍼", "🧑‍🍼", "👼", "🎅", "🤶", "🧑‍🎄", "🦸", "🦸‍♂️", "🦸‍♀️", "🦹", "🦹‍♂️", "🦹‍♀️", "🧙",
        "🧙‍♂️", "🧙‍♀️", "🧚", "🧚‍♂️", "🧚‍♀️", "🧛", "🧛‍♂️", "🧛‍♀️", "🧜", "🧜‍♂️", "🧜‍♀️", "🧝", "🧝‍♂️",
        "🧝‍♀️", "💆", "💆‍♂️", "💆‍♀️", "💇", "💇‍♂️", "💇‍♀️", "🚶", "🚶‍♂️", "🚶‍♀️", "🧍", "🧍‍♂️", "🧍‍♀️",
        "🧎", "🧎‍♂️", "🧎‍♀️", "🧑‍🦯", "👨‍🦯", "👩‍🦯", "🧑‍🦼", "👨‍🦼", "👩‍🦼", "🧑‍🦽", "👨‍🦽",
        "👩‍🦽", "🏃", "🏃‍♂️", "🏃‍♀️", "💃", "🕺", "🕴️", "🧖", "🧖‍♂️", "🧖‍♀️", "🧗", "🧗‍♂️", "🧗‍♀️",
        "🏇", "🏂", "🏌️", "🏌️‍♂️", "🏌️‍♀️", "🏄", "🏄‍♂️", "🏄‍♀️", "🚣", "🚣‍♂️", "🚣‍♀️", "🏊", "🏊‍♂️",
        "🏊‍♀️", "⛹️", "⛹️‍♂️", "⛹️‍♀️", "🏋️", "🏋️‍♂️", "🏋️‍♀️", "🚴", "🚴‍♂️", "🚴‍♀️", "🚵", "🚵‍♂️", "🚵‍♀️",
        "🤸", "🤸‍♂️", "🤸‍♀️", "🤽", "🤽‍♂️", "🤽‍♀️", "🤾", "🤾‍♂️", "🤾‍♀️", "🤹", "🤹‍♂️", "🤹‍♀️", "🧘",
        "🧘‍♂️", "🧘‍♀️", "🛀", "🛌"
    };
}
