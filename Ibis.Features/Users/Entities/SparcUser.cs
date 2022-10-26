namespace Ibis.Features.Users;

public class SparcUser : SparcRoot<string>
{
    public string? SecurityStamp
    {
        get; set;
    }

    public string? UserName { get; set; }
}