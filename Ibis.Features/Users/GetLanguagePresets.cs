using Ibis.Features.Users.Entities;

namespace Ibis.Features.Users;

public class GetLanguagePresets : Feature<string, List<LanguagePreset>>
{
    public IRepository<User> Users { get; }
    public IRepository<LanguagePreset> LanguagePresets { get; }

    public GetLanguagePresets(IRepository<User> users, IRepository<LanguagePreset> languagePesets)
    {
        LanguagePresets = languagePesets;
        Users = users;
    }

    public override async Task<List<LanguagePreset>> ExecuteAsync(string userId)
    {
        var user = await Users.FindAsync(userId);
        var userPresets = LanguagePresets.Query.Where(x => x.UserId == userId);
        if (user != null) return userPresets.ToList();
        else return new List<LanguagePreset>();
    }
}
