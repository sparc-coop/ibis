using Ibis.Features.Users.Entities;

namespace Ibis.Features.Users;

public record UpdateLanguagePresetRequest(string Id, string UserId, string Name, string LanguageId, string DialectId, string VoiceId, string VoiceGender, bool IsDefault, string? RoomId);
public class UpdateLanguagePreset : Feature<UpdateLanguagePresetRequest, LanguagePreset>
{
    public IRepository<User> Users { get; }
    public IRepository<LanguagePreset> LanguagePresets { get; }
    public IRepository<LanguagePresetRoomPair> LanguagePresetRoomPairs { get; }

    public UpdateLanguagePreset(IRepository<User> users, IRepository<LanguagePreset> languagePresets, IRepository<LanguagePresetRoomPair> languagePresetRoomPairs)
    {
        Users = users;
        LanguagePresets = languagePresets;
        LanguagePresetRoomPairs = languagePresetRoomPairs;
    }

    public override async Task<LanguagePreset> ExecuteAsync(UpdateLanguagePresetRequest request)
    {
        User user = await Users.FindAsync(request.UserId);
        LanguagePreset preset = await LanguagePresets.FindAsync(request.Id);

        if (preset != null && user != null)
        {
            // check if updatedPreset is set to be default preset
            // update currentDefault and LanguagePresetRoomPairs using default
            var userLanguagePresets = LanguagePresets.Query.Where(x => x.UserId == request.UserId);
            if (preset.IsDefault == true)
            {
                // change current default
                var currentDefault = userLanguagePresets.First(x => x.IsDefault == true);
                currentDefault.IsDefault = false;
                await LanguagePresets.UpdateAsync(currentDefault);

                var usingDefault = LanguagePresetRoomPairs.Query.Where(x => x.UserId == request.UserId && x.PresetId == currentDefault.Id);
                foreach (var item in usingDefault)
                {
                    LanguagePresetRoomPair newItem = new LanguagePresetRoomPair(request.UserId, currentDefault.Id, item.RoomId, DateTime.Now);
                    user.LanguagePresetsForRooms.Remove(item);
                    user.LanguagePresetsForRooms.Add(newItem);
                }
                await Users.UpdateAsync(user);
            }

            // update preset
            preset.Name = request.Name;
            preset.LanguageId = request.LanguageId;
            preset.DialectId = request.DialectId;
            preset.OutgoingVoiceId = request.VoiceId;
            preset.OutgoingVoiceGender = request.VoiceGender;
            preset.IsDefault = request.IsDefault;
            preset.DateModified = DateTime.Now;

            await LanguagePresets.UpdateAsync(preset);
            return preset;
        }
        return preset;
    }
}