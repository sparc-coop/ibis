namespace Ibis.Messages
{
    public interface ISharedService
    {
        string FileUrl { get; set; }
    }

    public class SharedService : ISharedService
    {
        public string FileUrl { get; set; }
    }
}
