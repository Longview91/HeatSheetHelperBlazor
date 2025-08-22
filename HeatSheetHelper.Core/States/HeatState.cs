using HeatSheetHelper.Core.Shared;
using System.Text.RegularExpressions;

namespace HeatSheetHelper.Core.States
{
    internal class HeatState : State
    {
        public override void HandleLine(string line, ref SwimEvent currentEvent, ref HeatInfo currentHeat, ref List<SwimEvent> events)
        {
            // Save previous heat if exists
            if (currentHeat != null && currentHeat.HeatNumber != 100 && currentEvent != null)
            {
                currentEvent.Heats.Add(currentHeat);
                currentHeat = null;
            }

            // Parse heat number and start time
            var heatMatch = Regex.Match(line, @"HEAT\s+(\d+)");
            int heatNumber = heatMatch.Success ? int.Parse(heatMatch.Groups[1].Value) : 0;
            var timeMatch = Regex.Match(line, @"([1-9]|10|11|12):\d{2}\s*(AM|PM)");
            string startTime = timeMatch.Success ? timeMatch.Value.Trim() : "";

            currentHeat = new HeatInfo
            {
                HeatNumber = heatNumber,
                StartTime = startTime,
                LaneInfos = new List<LaneInfo>().AsQueryable()
            };
        }
    }
}
