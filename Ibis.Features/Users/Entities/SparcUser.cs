namespace Ibis.Features.Users;

public class SparcUser : Root<string>
{
    public string? SecurityStamp
    {
        get; set;
    }

    public string? UserName { get; set; }
}