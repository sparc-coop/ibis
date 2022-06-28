using Ibis.Features.Users.Entities;

namespace Ibis.Features.Users;

public record UpdateLanguagePresetRequest(string Id, string UserId, string RoomId, string Name, string LanguageId, string DialectId, string VoiceId, string VoiceGender, bool IsDefault);
public class UpdateLanguagePresetForRoomUser : Feature<UpdateLanguagePresetRequest, LanguagePreset>
{
    public IRepository<LanguagePreset> LanguagePresets { get; }
    public IRepository<User> Users { get; }


    public UpdateLanguagePresetForRoomUser(IRepository<LanguagePreset> languagePresets, IRepository<User> users)
    {
        LanguagePresets = languagePresets;
        Users = users;
    }

    public override async Task<LanguagePreset> ExecuteAsync(UpdateLanguagePresetRequest request)
    {
        User user = await Users.FindAsync(request.UserId);
        LanguagePreset preset = await LanguagePresets.FindAsync(request.Id);
        if (preset != null && user != null)
        {
            // check if a preset is already applied to a room (checking preset/room pairs under User.LanguagePresetsForRooms)
            // if existing AND a new preset is being applied to the room
            // remove this pair from the list
            var existing = user.LanguagePresetsForRooms.First(x => x.RoomId == request.RoomId);
            if (existing != null && existing.PresetId != request.RoomId)
            {
                user.LanguagePresetsForRooms.Remove(existing);
            }

            // if new preset is set as default, update all other presets

            // update preset
            preset.Name = request.Name;
            preset.UserId = request.UserId;
            preset.LanguageId = request.LanguageId;
            preset.DialectId = request.DialectId;
            preset.OutgoingVoiceId = request.VoiceId;
            preset.OutgoingVoiceGender = request.VoiceGender;
            preset.IsDefault = request.IsDefault;
            preset.DateModified = DateTime.Now;

            // add updatedPreset to User list of preset/room pairs (User.LanguagePresetsForRooms)
            LanguagePresetRoomPair pair = new(request.UserId, preset.Id, request.RoomId);
            user.LanguagePresetsForRooms.Add(pair);

            await LanguagePresets.UpdateAsync(preset);
            return preset;
        }
        return preset;
    }
}
