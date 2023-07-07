namespace Ibis._Plugins.Translation;

public static class BrailleConverter
{
    public static string Convert(string? text) =>
        text == null
        ? string.Empty
        : BrailleAscii.Aggregate(text, (current, item) => current.Replace(item.Key, item.Value));

    private static readonly Dictionary<string, string> BrailleAscii = new()
    {
            { "and", "&" },
            { "for", "=" },
            { "of", "(" },
            { "the", "!" },
            { "with", ")" },
            { "ing", "+" },

            { "ch", "*" },
            { "gh", "<" },
            { "sh", "%" },
            { "th", "?" },
            { "wh", ":" },
            { "ed", "$" },
            { "er", "]" },
            { "ou", "\\" },
            { "ow", "[" },
            { ",", "1" },
            { ";", "2" },
            { ":", "3" },
            { ".", "4" },
            { "en", "5" },
            { "!", "6" },
            { "(", "7" },
            { "?", "8" },
            { "in", "9" },
            { "'", "0" },
            { "st", "/" },
            { "#", "#" },
            { "ar", ">" },
            { " ", "(space)" }
        };

    private static readonly Dictionary<char, string> BrailleUnicode = new()
        {
            { 'a', "⠁" },
            { 'b', "⠃" },
            { 'c', "⠉" },
            { 'd', "⠙" },
            { 'e', "⠑" },
            { 'f', "⠋" },
            { 'g', "⠛" },
            { 'h', "⠓" },
            { 'i', "⠊" },
            { 'j', "⠚" },
            { 'k', "⠅" },
            { 'l', "⠇" },
            { 'm', "⠍" },
            { 'n', "⠝" },
            { 'o', "⠕" },
            { 'p', "⠏" },
            { 'q', "⠟" },
            { 'r', "⠗" },
            { 's', "⠎" },
            { 't', "⠞" },
            { 'u', "⠥" },
            { 'v', "⠧" },
            { 'w', "⠺" },
            { 'x', "⠭" },
            { 'y', "⠽" },
            { 'z', "⠵" },
            { ' ', "  " }
        };
}
