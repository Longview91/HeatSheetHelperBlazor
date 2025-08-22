namespace HeatSheetHelper.Core.Shared
{
    public class HeatInfo
    {
        public int HeatNumber { get; set; }
        public string StartTime { get; set; }
        public IQueryable<LaneInfo> LaneInfos { get; set; }

        public HeatInfo()
        {
            HeatNumber = 0;
            StartTime = string.Empty;
            LaneInfos = new List<LaneInfo>().AsQueryable();
        }
    }
}