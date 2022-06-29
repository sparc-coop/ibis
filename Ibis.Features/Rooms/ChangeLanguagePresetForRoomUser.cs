using Ibis.Features.Users.Entities;

namespace Ibis.Features.Rooms;

public record ChangeLanguagePresetRequest(string UserId, string PresetId, string RoomId);

public class ChangeLanguagePresetForRoomUser : Feature<ChangeLanguagePresetRequest, LanguagePreset>
{
    public IRepository<User> Users { get; }
    public IRepository<LanguagePreset> LanguagePresets { get; }
    public IRepository<LanguagePresetRoomPair> LanguagePresetRoomPairs { get; }


    public ChangeLanguagePresetForRoomUser(IRepository<User> users, IRepository<LanguagePreset> languagePresets, IRepository<LanguagePresetRoomPair> languagePresetRoomPairs)
    {
        Users = users;
        LanguagePresets = languagePresets;
        LanguagePresetRoomPairs = languagePresetRoomPairs;
    }

    public override async Task<LanguagePreset> ExecuteAsync(ChangeLanguagePresetRequest request)
    {
        User user = await Users.FindAsync(request.UserId);
        LanguagePreset preset = await LanguagePresets.FindAsync(request.PresetId);
        if (user != null && preset != null)
        {
            // check if a preset is already applied to a room (checking preset/room pairs under User.LanguagePresetsForRooms)
            // if existing AND a new preset is being applied to the room, remove pair from list, add new pair
            var existing = user.LanguagePresetsForRooms.First(x => x.RoomId == request.RoomId);
            if (existing != null && existing.PresetId != request.RoomId)
            {
                LanguagePresetRoomPair pair = new(request.UserId, request.PresetId, request.RoomId, DateTime.Now);
                user.LanguagePresetsForRooms.Remove(existing);
                user.LanguagePresetsForRooms.Add(pair);
            }
            return preset;
        }
        return preset;
    }
}

