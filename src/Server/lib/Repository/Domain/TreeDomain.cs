namespace Server.lib.Repository.Domain
{
    public enum TreeType
    {
        Normal,
        Binary
    }
    public class TreeDomain
    {
        public string uuid { get; set; }
        public TreeType type { get; set; }
    }
}