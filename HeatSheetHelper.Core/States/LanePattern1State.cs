using HeatSheetHelper.Core.Constants;
using HeatSheetHelper.Core.Helpers;
using HeatSheetHelper.Core.Interfaces;
using HeatSheetHelper.Core.Shared;
using System.Text.RegularExpressions;

namespace HeatSheetHelper.Core.States
{
    internal class LanePattern1State : State
    {
        public ISwimmerFunctions _swimmerFunctions { get; set; } = new SwimmerFunctions();
        public override void HandleLine(string line, ref SwimEvent currentEvent, ref HeatInfo currentHeat, ref List<SwimEvent> events)
        {
            var regex = new Regex(RegexExpressions.singleSwimmerPatternMain);
            var match = regex.Match(line);
            if (match.Success)
            {
                string name = match.Groups["name"].Value.Trim();
                if (name.Contains(","))
                {
                    name = _swimmerFunctions.SwapLastCommaFirstToFirstLast(name);
                }

                var laneInfo = new LaneInfo
                {
                    LaneNumber = int.TryParse(match.Groups["laneNumber"].Value.Trim(), out int ln) ? ln : 0,
                    TeamName = match.Groups["teamName"].Value.Trim(),
                    SeedTime = match.Groups["seedTime"].Value.Trim(),
                    SwimmerName = name,
                    SwimmerAge = int.TryParse(match.Groups["age"].Value.Trim(), out int age) ? age : 0
                };
                if (currentHeat == null && currentEvent != null)
                {
                    // Only create and add sign-in heat if it doesn't exist
                    currentHeat = currentEvent.Heats.FirstOrDefault(h => h.HeatNumber == 100);
                    if (currentHeat == null)
                    {
                        currentHeat = new HeatInfo
                        {
                            HeatNumber = 100,
                            StartTime = string.Empty,
                            LaneInfos = new List<LaneInfo>().AsQueryable()
                        };
                        currentEvent.Heats.Add(currentHeat);
                    }
                }
                if (currentHeat != null)
                {
                    var lanes = currentHeat.LaneInfos.ToList();
                    lanes.Add(laneInfo);
                    currentHeat.LaneInfos = lanes.AsQueryable();
                }
            }
        }
    }
}
