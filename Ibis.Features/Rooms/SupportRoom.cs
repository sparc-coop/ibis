namespace Ibis.Rooms;

public class SupportRoom : Room
{
    // For Ibis Support Rooms
    public bool? IsSupportRoom { get; private set; }
    public bool? ToBeResolved { get; private set; }
    public bool? IsResolved { get; private set; }
    public bool? IsGeneralSupport { get; private set; }
    public bool? IsBug { get; private set; }
    public bool? IsAccountIssue { get; private set; }
}
