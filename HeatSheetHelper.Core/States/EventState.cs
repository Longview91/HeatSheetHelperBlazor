using HeatSheetHelper.Core.Shared;
using System.Text.RegularExpressions;

namespace HeatSheetHelper.Core.States;
internal class EventState : State
{
    public override void HandleLine(string line, ref SwimEvent currentEvent, ref HeatInfo currentHeat, ref List<SwimEvent> events)
    {
        // Save previous event if exists
        if (currentEvent != null)
        {
            // Save any pending heat to the previous event
            if (currentHeat != null && currentHeat.HeatNumber != 100)
            {
                currentEvent.Heats.Add(currentHeat);
                currentHeat = null;
            }
            events.Add(currentEvent);
        }

        // Parse event number and details
        int eventNumber = 0;
        string eventDetails = "";
        var match = Regex.Match(line, @"(\d+)\s*(.*)");
        if (match.Success)
        {
            eventNumber = int.Parse(match.Groups[1].Value);
            eventDetails = match.Groups[2].Value.Trim();
        }

        currentEvent = new SwimEvent
        {
            EventNumber = eventNumber,
            EventDetails = eventDetails,
            Heats = new List<HeatInfo>()
        };
        currentHeat = null;
        if (line.Contains("RELAY"))
        {
            currentEvent.IsRelay = true;
        }
    }
}
