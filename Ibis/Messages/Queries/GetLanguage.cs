using Ardalis.Specification;
using Microsoft.AspNetCore.Localization;

namespace Ibis.Messages.Queries
{
    public class GetLanguage : Specification<Language>
    {
        public GetLanguage(HttpRequest? request, Room? room, User? user)
        {             
            var language = request?.HttpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture?.Culture?.TwoLetterISOLanguageName
                        ?? user?.Avatar.Language
                        ?? room?.Languages.First().Id
                        ?? "en";
        
            Query.Where(x => x.Id == language);
        }
    }
}
