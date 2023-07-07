using Ibis._Plugins.Billing;
using Ibis._Plugins.Speech;
using Ibis._Plugins.Translation;
using Microsoft.AspNetCore.OutputCaching;

namespace Ibis.Users;

public class Users : BlossomAggregate<User>
{
    public Users()
    {
        GetAsync = (User user) => user.Avatar;
        UpdateAsync = (User user, UserAvatar avatar) => user.UpdateAvatar(avatar);
    }
    
    [OutputCache(Duration = 3600)]
    public UserAvatar.GetEmojisResponse GetEmojis() => UserAvatar.AllEmojis();

    [OutputCache(Duration = 3600)]
    public async Task<List<Language>> GetLanguages(ISpeaker speaker, ITranslator translator) => await speaker.GetLanguagesAsync(translator);
   
    public async Task<bool> ProcessPaymentIntent(HttpRequest request, StripeBiller biller) => await biller.ProcessPaymentIntentAsync(request);
}
