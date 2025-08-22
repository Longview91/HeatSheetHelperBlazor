using HeatSheetHelper.Core.Constants;
using HeatSheetHelper.Core.Interfaces;
using HeatSheetHelper.Core.Shared;
using HeatSheetHelper.Core.Helpers;
using HeatSheetHelper.Core.States;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace HeatSheetHelper.Core.Helpers
{
    public class SwimmerFunctions : ISwimmerFunctions
    {
        private StateContext _context = new StateContext(new SkipLineState()); // Context to manage state transitions

        public SwimMeet ParseHeatSheetToEvents(List<string> heatSheet)
        {
            SwimMeet swimMeet = new();
            var events = new List<SwimEvent>();
            SwimEvent currentEvent = null;
            HeatInfo currentHeat = null;
            int currentRelayLane = 0;
            string currentRelayTeamName = null;
            string currentRelayTeamLetter = null;
            string currentRelaySeedTime = null;

            foreach (var dirtyLine in heatSheet)
            {
                string line = CleanseTheData(dirtyLine);

                // This will return as SkipLineState if the line is not relevant and we can use that to determine if we should
                // check for relay patterns or not
                _context.TransitionTo(new StateFactory().CreateState(line));
                
                // Relay team line
                if (_context.GetCurrentState() is SkipLineState && Regex.Match(line, RegexExpressions.relayTeamPattern).Success)
                {
                    var match = Regex.Match(line, RegexExpressions.relayTeamPattern);
                    currentRelaySeedTime = match.Groups["seedTime"].Value.Trim();
                    currentRelayTeamName = match.Groups["teamName"].Value.Trim();
                    currentRelayTeamLetter = match.Groups["relayTeamLetter"].Value.Trim();
                    currentRelayLane = int.TryParse(match.Groups["laneNumber"].Value.Trim(), out int ln) ? ln : 0;
                    _context.TransitionTo(new SkipLineState());
                }
                // Relay swimmer line
                else if (_context.GetCurrentState() is SkipLineState && Regex.Match(line, RegexExpressions.relaySwimmerPattern).Success)
                {
                    ParseRelaySwimmers(line, currentHeat, currentRelayLane, currentRelayTeamName, currentRelayTeamLetter, currentRelaySeedTime, RegexExpressions.relaySwimmerPattern);
                    _context.TransitionTo(new SkipLineState());
                }
                _context.GetCurrentState().HandleLine(line, ref currentEvent, ref currentHeat, ref events);
            }
            // Add any remaining heat to the last event
            if (currentHeat != null && currentHeat.HeatNumber != 100 && currentEvent != null)
            {
                currentEvent.Heats.Add(currentHeat);
                currentHeat = null;
            }

            // Add the last event if it exists
            if (currentEvent != null)
                events.Add(currentEvent);

            swimMeet.SwimEvents = events;
            FillMissingHeatStartTimes(swimMeet);

            return swimMeet;
        }

        public string SwapLastCommaFirstToFirstLast(string name)
        {
            var names = name.Split(',');
            name = names[1].Trim() + " " + names[0].Trim();
            return name;
        }
        public Tuple<string, string, string, bool> ParseRelaySwimmerEventInfo(string line, string pattern)
        {
            Regex regex = new(pattern);
            Match match = regex.Match(line);

            string seedTime = match.Groups["seedTime"].Value.Trim();
            string teamName = match.Groups["teamName"].Value.Trim(); ;
            string laneNumber = match.Groups["laneNumber"].Value.Trim(); ;
            var infoToReturn = new Tuple<string, string, string, bool>(seedTime, teamName, laneNumber, false);
            return infoToReturn;
        }
        public void ParseRelaySwimmers(string line, HeatInfo currentHeat, int currentRelayLane, string currentRelayTeamName, string currentRelayTeamLetter, string currentRelaySeedTime, string relayPattern)
        {
            if (currentHeat == null) return;

            Regex regex = new(relayPattern);
            Match match = regex.Match(line);

            if (!match.Success) return;

            // Parse first swimmer
            var firstName = match.Groups["name1"].Value.Trim();
            if (firstName.Contains(','))
                firstName = SwapLastCommaFirstToFirstLast(firstName);

            var firstLaneInfo = new LaneInfo
            {
                LaneNumber = currentRelayLane,
                TeamName = currentRelayTeamName,
                RelayTeamLetter = currentRelayTeamLetter,
                SeedTime = currentRelaySeedTime,
                SwimmerName = firstName,
                SwimmerAge = int.TryParse(match.Groups["age1"].Value.Trim(), out int age1) ? age1 : 0,
                isAlternate = false
            };

            // Parse second swimmer
            var secondName = match.Groups["name2"].Value.Trim();
            if (secondName.Contains(','))
                secondName = SwapLastCommaFirstToFirstLast(secondName);

            var secondLaneInfo = new LaneInfo
            {
                LaneNumber = firstLaneInfo.LaneNumber, // Same lane for relay
                TeamName = firstLaneInfo.TeamName,
                RelayTeamLetter = firstLaneInfo.RelayTeamLetter,
                SeedTime = firstLaneInfo.SeedTime,
                SwimmerName = secondName,
                SwimmerAge = int.TryParse(match.Groups["age2"].Value.Trim(), out int age2) ? age2 : 0,
                isAlternate = false
            };

            // Add both swimmers to the heat's lanes
            var lanes = currentHeat.LaneInfos.ToList();
            lanes.Add(firstLaneInfo);
            lanes.Add(secondLaneInfo);
            currentHeat.LaneInfos = lanes.AsQueryable();
        }
        public string CleanseTheData(string line)
        {
            line = Regex.Replace(line.ToUpper(), @"Β", "F"); //@"[^\d\s\w,:\-.'\/\!\@\#\$\%\^\&\*\(\)]", "?");
            return line.Replace("BUTTERBLY", "BUTTERFLY");
        }
        public void FillMissingHeatStartTimes(SwimMeet swimMeet)
        {
            foreach (var swimEvent in swimMeet.SwimEvents)
            {
                var heats = swimEvent.Heats;
                for (int i = 0; i < heats.Count; i++)
                {
                    var heat = heats[i];
                    if (string.IsNullOrWhiteSpace(heat.StartTime))
                    {
                        // Find previous heat with a time
                        int prevIdx = i - 1;
                        string prevTime = null;
                        while (prevIdx >= 0)
                        {
                            if (!string.IsNullOrWhiteSpace(heats[prevIdx].StartTime))
                            {
                                prevTime = heats[prevIdx].StartTime;
                                break;
                            }
                            prevIdx--;
                        }

                        // Find next heat with a time
                        int nextIdx = i + 1;
                        string nextTime = null;
                        while (nextIdx < heats.Count)
                        {
                            if (!string.IsNullOrWhiteSpace(heats[nextIdx].StartTime))
                            {
                                nextTime = heats[nextIdx].StartTime;
                                break;
                            }
                            nextIdx++;
                        }

                        // Calculate midpoint time
                        if (prevTime != null && nextTime != null)
                        {
                            if (TryParseTime(prevTime, out var prevDt) && TryParseTime(nextTime, out var nextDt))
                            {
                                // Handle day wrap-around if needed
                                if (nextDt < prevDt)
                                    nextDt = nextDt.AddDays(1);

                                var midTicks = prevDt.Ticks + (nextDt.Ticks - prevDt.Ticks) / 2;
                                var midTime = new DateTime(midTicks);
                                heat.StartTime = midTime.ToString("h:mm tt");
                            }
                        }
                        else if (prevTime != null && TryParseTime(prevTime, out var prevDtOnly))
                        {
                            heat.StartTime = prevDtOnly.ToString("h:mm tt");
                        }
                        else if (nextTime != null && TryParseTime(nextTime, out var nextDtOnly))
                        {
                            heat.StartTime = nextDtOnly.ToString("h:mm tt");
                        }
                        // else leave blank
                    }
                }
            }
        }
        // Helper to parse "h:mm AM/PM" to DateTime (date part is ignored)
        public bool TryParseTime(string time, out DateTime dt)
        {
            return DateTime.TryParseExact(
                time.Trim(),
                new[] { "h:mm tt", "hh:mm tt", "h:mmtt", "hh:mmtt" },
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out dt
            );
        }
    }
}
