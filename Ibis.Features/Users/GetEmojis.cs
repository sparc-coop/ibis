namespace Ibis.Features.Users;

public record GetEmojisResponse(List<string> Skintones, List<string> Emojis);
public class GetEmojis : Feature<GetEmojisResponse>
{
    public override Task<GetEmojisResponse> ExecuteAsync()
    {
        GetEmojisResponse result = new(UserAvatar.SkinTones(), UserAvatar.Emojis());
        return Task.FromResult(result);
    }
}
