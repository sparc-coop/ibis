using Ibis.Features.Users.Entities;

namespace Ibis.Features.Users;

public record CreateLanguagePresetRequest(string Name, string UserId, string LanguageId, string DialectId, string VoiceId, string VoiceGender, bool IsDefault);

public class CreateLanguagePreset : Feature<CreateLanguagePresetRequest, LanguagePreset>
{
    public IRepository<User> Users { get; }
    public IRepository<LanguagePreset> LanguagePresets { get; }
    public IRepository<LanguagePresetRoomPair> LanguagePresetRoomPairs { get; }

    public CreateLanguagePreset(IRepository<User> users, IRepository<LanguagePreset> languagePresets, IRepository<LanguagePresetRoomPair> languagePresetRoomPairs)
    {
        Users = users;
        LanguagePresets = languagePresets;
        LanguagePresetRoomPairs = languagePresetRoomPairs;
    }

    public override async Task<LanguagePreset> ExecuteAsync(CreateLanguagePresetRequest request)
    {
        User user = await Users.FindAsync(request.UserId);
        var newPreset = new LanguagePreset(request.Name, request.UserId, request.LanguageId, request.DialectId, request.VoiceId, request.VoiceGender, request.IsDefault);

        if (user != null)
        {
            await LanguagePresets.AddAsync(newPreset);

            // check and update default preset
            var userLanguagePresets = LanguagePresets.Query.Where(x => x.UserId == request.UserId);
            if (newPreset.IsDefault == true)
            {
                // change current default
                var currentDefault = userLanguagePresets.First(x => x.IsDefault == true);
                currentDefault.IsDefault = false;
                await LanguagePresets.UpdateAsync(currentDefault);

                // update all presetRoomPairs using currentDefault to use newPreset
                var usingDefault = user.LanguagePresetsForRooms.Where(x => x.PresetId == currentDefault.Id);
                foreach (var item in usingDefault)
                {
                    LanguagePresetRoomPair newItem = new LanguagePresetRoomPair(request.UserId, newPreset.Id, item.RoomId, DateTime.Now);
                    user.LanguagePresetsForRooms.Remove(item);
                    user.LanguagePresetsForRooms.Add(newItem);
                }
                await Users.UpdateAsync(user);
            }
            
            return newPreset;
        }
        return newPreset;
    }
}
