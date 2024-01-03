﻿using Microsoft.AspNetCore.OutputCaching;

namespace Ibis.Users;

public class Users : BlossomRoot<User>
{
    public Users(IRepository<User> users) : base(users)
    {
        Api.GetAsync = (User user) => user.Avatar;
        Api.UpdateAsync = (User user, UserAvatar avatar) => user.UpdateAvatar(avatar);
    }
    
    public record GetEmojisResponse(List<string> Skintones, List<string> Emojis, List<string> Colors);
    [OutputCache(Duration = 3600)]
    public Task<GetEmojisResponse> GetEmojis()
    {
        GetEmojisResponse result = new(UserAvatar.SkinTones(), UserAvatar.Emojis(), UserAvatar.BackgroundColors());
        return Task.FromResult(result);
    }

    [OutputCache(Duration = 3600)]
    public async Task<List<Language>> GetLanguages(ITranslator translator, ISpeaker speaker)
    {
        var languages = await translator.GetLanguagesAsync();
        var voices = await speaker.GetVoicesAsync();

        var result = new List<Language>();
        foreach (var language in languages)
        {
            var voicesByDialect = voices
                .Where(x => x.Locale.Split("-").First() == language.Id)
                .GroupBy(x => x.Locale);

            foreach (var dialect in voicesByDialect)
                language.AddDialect(dialect.Key, dialect.ToList());

            result.Add(language);
        }

        return result;
    }
}