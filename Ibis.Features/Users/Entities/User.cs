using Sparc.Core;
using Newtonsoft.Json;

namespace Ibis.Features;
public class User : Root<string>
{
    public User()
    {
        Id = string.Empty;
        UserId = Id;
        PrimaryLanguageId = string.Empty;
        LanguagesSpoken = new();
    }

    public string UserId { get { return Id; } set { Id = value; } }
    private string? _email;
    public string? Email
    {
        get { return _email; }
        set
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                _email = null;
                return;
            }

            _email = value.Trim().ToLower();
        }
    }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    [JsonIgnore]
    public string FullName => $"{FirstName} {LastName}";
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
    public string PrimaryLanguageId { get; set; }
    public List<Language> LanguagesSpoken { get; set; }
}
