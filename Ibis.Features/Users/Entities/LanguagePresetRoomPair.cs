namespace Ibis.Features.Users.Entities;

public class LanguagePresetRoomPair
{
    public string UserId { get; set; }
    public string PresetId { get; set; }
    public string RoomId { get; set; }
    public DateTime DateModified { get; set; }

    public LanguagePresetRoomPair(string userId, string presetId, string roomId)
    {
        UserId = userId;
        PresetId = presetId;
        RoomId = roomId;
        DateModified = DateTime.Now;
    }

    public void SetPreset (string presetId) => PresetId = presetId;
}
