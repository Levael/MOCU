namespace DaemonsRelated
{
    public class DaemonErrorReport
    {
        public string message { get; set; }
        public bool isFatal { get; set; }
        public object payload { get; set; }
    }
}