using Ibis.Features.Users.Entities;

namespace Ibis.Features.Users;

public class DeleteLanguagePreset : Feature<string, string>
{
    public IRepository<User> Users { get; }
    public IRepository<LanguagePreset> LanguagePresets { get; }

    public DeleteLanguagePreset(IRepository<User> users, IRepository<LanguagePreset> languagePresets)
    {
        Users = users;
        LanguagePresets = languagePresets;
    }

    public override async Task<string> ExecuteAsync(string Id)
    {
        LanguagePreset preset = await LanguagePresets.FindAsync(Id);
        if (preset != null)
        {
            // check if preset is default, default must be changed before deleted
            if (preset.IsDefault == true)
            {
                return "Default Language Preset cannot be deleted. Please set another preset as default before deleting.";
            }

            // check if preset is being used in any user rooms
            User user = await Users.FindAsync(preset.UserId);
            var beingUsed = user.LanguagePresetsForRooms.Where(x => x.PresetId == preset.Id);
            LanguagePreset defaultPreset = LanguagePresets.Query.First(x => x.UserId == user.Id && x.IsDefault == true);
            
            foreach (var item in beingUsed)
            {
                var newItem = new LanguagePresetRoomPair(user.Id, defaultPreset.Id, item.RoomId, DateTime.Now);
                user.LanguagePresetsForRooms.Remove(item);
                user.LanguagePresetsForRooms.Add(newItem);
            }

            await LanguagePresets.DeleteAsync(preset);
            return "Language Preset Deleted";
        }

        return "Language Preset Not Found";
    }
}
