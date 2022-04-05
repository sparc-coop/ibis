using Sparc.Core;
using System.Globalization;

namespace Ibis.Features.Conversations.Entities;

public class Conversation : Root<string>
{
    public string ConversationId { get; set; }
    public string Name { get; set; }
    public string HostUserId { get; set; }
    public List<Language> Languages { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? LastActiveDate { get; set; }
    public List<ActiveUser> ActiveUsers { get; internal set; }
    public List<Translation> Translations { get; private set; }
    public string? AudioId { get; set; }

    private Conversation() 
    { 
        Id = Guid.NewGuid().ToString();
        ConversationId = Id;
        Name = "New Conversation";
        HostUserId = "";
        Languages = new();
        StartDate = DateTime.UtcNow;
        LastActiveDate = DateTime.UtcNow;
        ActiveUsers = new();
    }

    public Conversation(string name, string hostUserId) : this()
    {
        Name = name;
        HostUserId = hostUserId;
    }

    public void AddLanguage(string language)
    {
        if (Languages.Any(x => x.Name == language))
            return;

        Languages.Add(new(language));
    }

    public void AddUser(string userId, string language, string? phoneNumber = null)
    {
        if (!ActiveUsers.Any(x => x.UserId == userId))
            ActiveUsers.Add(new(userId, DateTime.UtcNow, language, phoneNumber));
    }

    public void RemoveUser(string userId)
    {
        ActiveUsers.RemoveAll(x => x.UserId == userId);
    }

    public void SetAudio(string audioId) => AudioId = audioId;
}

public record ActiveUser(string UserId, DateTime JoinDate, string Language, string? PhoneNumber);