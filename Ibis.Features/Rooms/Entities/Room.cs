using System.Diagnostics;
using System.Text;

namespace Ibis.Rooms;

public record SourceMessage(string RoomId, string MessageId);
public record UserJoined(string RoomId, UserAvatar User) : Notification(RoomId);
public record UserLeft(string RoomId, UserAvatar User) : Notification(RoomId);

public class Room : BlossomEntity<string>
{
    public string RoomId { get; private set; }
    public string RoomType { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public UserAvatar HostUser { get; private set; }
    public List<UserAvatar> Users { get; private set; }
    public SourceMessage? SourceMessage { get; private set; }
    public List<Language> Languages { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? LastActiveDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public AudioMessage? Audio { get; private set; }

    private Room() 
    { 
        Id = Guid.NewGuid().ToString();
        RoomId = Id;
        RoomType = "Chat";
        Name = "";
        Slug = "";
        SetName("New Room");
        HostUser = new User().Avatar;
        Languages = new();
        StartDate = DateTime.UtcNow;
        LastActiveDate = DateTime.UtcNow;
        Users = new();
    }

    public Room(string name, string type, User hostUser) : this()
    {
        SetName(name);
        RoomType = type;
        HostUser = hostUser.Avatar;
    }

    public Room(Room room, Message message) : this()
    {
        // Create a subroom from a message

        SetName(room.Name);
        RoomType = room.RoomType;
        SourceMessage = new(room.Id, message.Id);
        //Languages = room.Languages;
        //ActiveUsers = room.ActiveUsers;
        //Translations = room.Translations;
    }

    public void AddLanguage(Language language)
    {
        if (Languages.Any(x => x.Id == language.Id))
            return;

        Languages.Add(language);
    }

    public void AddActiveUser(User user)
    {
        var activeUser = Users.FirstOrDefault(x => x.Id == user.Id);
        if (activeUser == null)
        {
            activeUser = user.Avatar;
            Users.Add(activeUser);
        }

        if (user.PrimaryLanguage != null)
            AddLanguage(user.PrimaryLanguage);
        
        Broadcast(new UserJoined(Id, activeUser));
    }

    public void RemoveActiveUser(User user)
    {
        var activeUser = Users.FirstOrDefault(x => x.Id == user.Id);
        if (activeUser != null)
            Broadcast(new UserLeft(Id, activeUser));
    }

    internal void InviteUser(UserAvatar user)
    {
        if (!Users.Any(x => x.Id == user.Id))
            Users.Add(user);
    }

    internal async Task<List<Message>> TranslateAsync(Message message, Translator translator, bool forceRetranslation = false)
    {
        var languagesToTranslate = forceRetranslation
            ? Languages.Where(x => x.Id != message.Language).ToList()
            : Languages.Where(x => !message.HasTranslation(x.Id)).ToList();

        if (!languagesToTranslate.Any())
            return new();

        try
        {
            var translatedMessages = await translator.TranslateAsync(message, languagesToTranslate);
        
        
        // Add reference to all the new translated messages
        foreach (var translatedMessage in translatedMessages)
            message.AddTranslation(translatedMessage);

        return translatedMessages;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            throw;
        }
    }

    internal async Task SpeakAsync(ISpeaker speaker, List<Message> messages)
    {
        Audio = await speaker.SpeakAsync(messages);        
    }

    internal void Close()
    {
        EndDate = DateTime.UtcNow;
    }

    internal void SetName(string title)
    {
        Name = title;
        Slug = UrlFriendly(Name);
    }

    // Adopted from https://stackoverflow.com/a/25486
    static string UrlFriendly(string title)
    {
        if (title == null) return "";

        const int maxlen = 80;
        int len = title.Length;
        bool prevdash = false;
        var sb = new StringBuilder(len);
        char c;

        for (int i = 0; i < len; i++)
        {
            c = title[i];
            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
            {
                sb.Append(c);
                prevdash = false;
            }
            else if (c >= 'A' && c <= 'Z')
            {
                // tricky way to convert to lowercase
                sb.Append((char)(c | 32));
                prevdash = false;
            }
            else if (c == ' ' || c == ',' || c == '.' || c == '/' ||
                c == '\\' || c == '-' || c == '_' || c == '=')
            {
                if (!prevdash && sb.Length > 0)
                {
                    sb.Append('-');
                    prevdash = true;
                }
            }
            else if ((int)c >= 128)
            {
                int prevlen = sb.Length;
                sb.Append(RemapInternationalCharToAscii(c));
                if (prevlen != sb.Length) prevdash = false;
            }
            if (i == maxlen) break;
        }

        if (prevdash)
            return sb.ToString()[..(sb.Length - 1)];
        else
            return sb.ToString();
    }

    public static string RemapInternationalCharToAscii(char c)
    {
        string s = c.ToString().ToLowerInvariant();
        if ("àåáâäãåą".Contains(s))
        {
            return "a";
        }
        else if ("èéêëę".Contains(s))
        {
            return "e";
        }
        else if ("ìíîïı".Contains(s))
        {
            return "i";
        }
        else if ("òóôõöøőð".Contains(s))
        {
            return "o";
        }
        else if ("ùúûüŭů".Contains(s))
        {
            return "u";
        }
        else if ("çćčĉ".Contains(s))
        {
            return "c";
        }
        else if ("żźž".Contains(s))
        {
            return "z";
        }
        else if ("śşšŝ".Contains(s))
        {
            return "s";
        }
        else if ("ñń".Contains(s))
        {
            return "n";
        }
        else if ("ýÿ".Contains(s))
        {
            return "y";
        }
        else if ("ğĝ".Contains(s))
        {
            return "g";
        }
        else if (c == 'ř')
        {
            return "r";
        }
        else if (c == 'ł')
        {
            return "l";
        }
        else if (c == 'đ')
        {
            return "d";
        }
        else if (c == 'ß')
        {
            return "ss";
        }
        else if (c == 'Þ')
        {
            return "th";
        }
        else if (c == 'ĥ')
        {
            return "h";
        }
        else if (c == 'ĵ')
        {
            return "j";
        }
        else
        {
            return "";
        }
    }
}
