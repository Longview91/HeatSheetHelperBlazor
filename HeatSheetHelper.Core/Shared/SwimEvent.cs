namespace HeatSheetHelper.Core.Shared
{
    public class SwimEvent
    {
        public int EventNumber { get; set; }
        public string EventDetails { get; set; }
        public List<HeatInfo> Heats { get; set; }
        public bool IsRelay { get; set; }
        public SwimEvent()
        {
            EventNumber = 0;
            EventDetails = string.Empty;
            Heats = new List<HeatInfo>();
            IsRelay = false;
        }
    }
}
