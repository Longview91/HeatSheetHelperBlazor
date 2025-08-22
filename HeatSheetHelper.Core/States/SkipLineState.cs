using HeatSheetHelper.Core.Shared;

namespace HeatSheetHelper.Core.States
{
    internal class SkipLineState : State
    {
        public override void HandleLine(string line, ref SwimEvent currentEvent, ref HeatInfo currentHeat, ref List<SwimEvent> events)
        {
            // Skip lines that are not relevant to the current event or heat
        }
    }
}
