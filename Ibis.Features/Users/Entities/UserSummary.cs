namespace Ibis.Features.Users;

public record UserSummary
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Initials { get; set; }
    public string Color { get; set; }
    public bool ReceivesSms { get; set; }
    public string Language { get; set; }
    public bool IsOnline { get; set; }
    public string? Pronouns { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }

    public UserSummary(User user)
    {
        Id = user.Id;
        Name = user.FullName;
        Initials = user.Initials;
        Color = user.Color;
        ReceivesSms = false;
        Language = user.PrimaryLanguageId;
        Pronouns = user.Pronouns;
        Description = user.Description;
        Image = user.ProfileImg;
    }

    public UserSummary(string email)
    {
        Id = email;
        Name = email;
        Initials = string.Empty;
        Color = string.Empty;
        Language = string.Empty;
    }
}
