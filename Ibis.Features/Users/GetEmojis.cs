using Microsoft.AspNetCore.OutputCaching;

namespace Ibis.Users;

public record GetEmojisResponse(List<string> Skintones, List<string> Emojis, List<string> Colors);
[OutputCache(Duration = 3600)]
public class GetEmojis : Feature<GetEmojisResponse>
{
    public override Task<GetEmojisResponse> ExecuteAsync()
    {
        GetEmojisResponse result = new(UserAvatar.SkinTones(), UserAvatar.Emojis(), UserAvatar.BackgroundColors());
        return Task.FromResult(result);
    }
}
