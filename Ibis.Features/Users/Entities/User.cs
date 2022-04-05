using Sparc.Core;
using Newtonsoft.Json;

namespace Ibis.Features;
public class User : Root<string>
{
    public User()
    {
        Id = string.Empty;
        UserId = Id;
        PrimaryLanguageId = string.Empty;
        DateCreated = DateTime.UtcNow;
        DateModified = DateTime.UtcNow;
        LanguagesSpoken = new();
        ActiveConversations = new();
    }

    public string UserId { get { return Id; } set { Id = value; } }
    private string? _email;
    public string? Email
    {
        get { return _email; }
        set
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                _email = null;
                return;
            }

            _email = value.Trim().ToLower();
        }
    }

    public string? PhoneNumber { get; set; }
    public string? FirstName { get; set; }

    internal void JoinConversation(string conversationId, string connectionId)
    {
        if (!ActiveConversations.Any(x => x.ConversationId == conversationId))
            ActiveConversations.Add(new(conversationId, connectionId, DateTime.UtcNow));
    }

    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    [JsonIgnore]
    public string FullName => $"{FirstName} {LastName}";
    [JsonIgnore]
    public string Initials => $"{FirstName?[0]}{LastName?[0]}";
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }

    internal string? LeaveConversation(string conversationOrConnectionId)
    {
        var conversationId = ActiveConversations.FirstOrDefault(x => x.ConversationId == conversationOrConnectionId || x.ConnectionId == conversationOrConnectionId)?.ConversationId;
        if (conversationId == null) return null;

        ActiveConversations.RemoveAll(x => x.ConversationId == conversationId);
        return conversationId;
    }

    public string PrimaryLanguageId { get; set; }
    public List<Language> LanguagesSpoken { get; set; }
    public List<ActiveConversation> ActiveConversations { get; set; }
}

public record ActiveConversation(string ConversationId, string ConnectionId, DateTime JoinDate);
