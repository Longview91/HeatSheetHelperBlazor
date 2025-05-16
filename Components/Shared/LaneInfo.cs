namespace HeatSheetHelperBlazor.Components.Shared
{
    public class LaneInfo
    {
        public int LaneNumber { get; set; }
        public string TeamName { get; set; }
        public string RelayTeamLetter { get; set; }
        public string SeedTime { get; set; }
        public string SwimmerName { get; set; }
        public int SwimmerAge { get; set; }
        public bool isAlternate { get; set; }

        public LaneInfo()
        {
            LaneNumber = 0;
            TeamName = string.Empty;
            RelayTeamLetter = string.Empty;
            SeedTime = string.Empty;
            SwimmerName = string.Empty;
            SwimmerAge = 0;
            isAlternate = false;
        }
        public void ClearLaneInfo()
        {
            LaneNumber = 0;
            TeamName = string.Empty;
            RelayTeamLetter = string.Empty;
            SeedTime = string.Empty;
            SwimmerName = string.Empty;
            SwimmerAge = 0;
            isAlternate = false;
        }
    }
}