namespace Ibis.Features.Users.Entities;

public class LanguagePreset
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string UserId { get; set; }
    public string LanguageId { get; set; } //Language.Name
    public string? DialectId { get; set; } //
    public string? OutgoingVoiceId { get; set; }
    public string OutgoingVoiceGender { get; set; }
    public bool IsDefault { get; set; }
    public List<string> RoomIds { get; private set; }

    private LanguagePreset()
    {
        Id = Guid.NewGuid().ToString();
        Name = "New Preset";
        UserId = "";
        LanguageId = "";
        DialectId = "";
        OutgoingVoiceId = "";
        OutgoingVoiceGender = "";
        IsDefault = false;
    }

    public LanguagePreset(string name, string user, string language, string dialect, string outgoingVoice, string outgoingVoiceGender, bool isDefault)
    {
        Name = name;
        UserId = user;
        LanguageId = language;
        DialectId = dialect;
        OutgoingVoiceId = outgoingVoice;
        OutgoingVoiceGender = outgoingVoiceGender;
        IsDefault = isDefault;
    }

    public void AddRoom(string roomId)
    {
        var existing = RoomIds.FindIndex(x => x == roomId);

        if (existing == -1)
            RoomIds.Add(roomId);
        else
            RoomIds[existing] = roomId;
    }
}