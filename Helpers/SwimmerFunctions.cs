using Dapper;
using HeatSheetHelper.Constants;
using HeatSheetHelperBlazor;
using HeatSheetHelperBlazor.Components.Shared;
using System.Data;
using System.Text.RegularExpressions;

namespace HeatSheetHelper.Helpers
{
    internal class SwimmerFunctions
    {
        internal static SwimMeet ParseHeatSheetToEvents(List<string> heatSheet)
        {
            SwimMeet swimMeet = new();
            var events = new List<SwimEvent>();
            SwimEvent currentEvent = null;
            HeatInfo currentHeat = null;

            foreach (var dirtyLine in heatSheet)
            {
                string line = CleanseTheData(dirtyLine);

                // Event line
                if (line.StartsWith('#') || line.StartsWith("EVENT"))
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
                // Heat line
                else if (Regex.Match(line, @"\bHEAT\s+(\d+)").Success)
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
                // Lane/Swimmer line (individual)
                else if (Regex.Match(line, RegexExpressions.singleSwimmerPatternMain).Success)
                {
                    var regex = new Regex(RegexExpressions.singleSwimmerPatternMain);
                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        var laneInfo = new LaneInfo
                        {
                            LaneNumber = int.TryParse(match.Groups["laneNumber"].Value.Trim(), out int ln) ? ln : 0,
                            TeamName = match.Groups["teamName"].Value.Trim(),
                            SeedTime = match.Groups["seedTime"].Value.Trim(),
                            SwimmerName = SwapLastCommaFirstToFirstLast(match.Groups["name"].Value.Trim()),
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
                // Lane/Swimmer line (secondary pattern)
                else if (Regex.Match(line, RegexExpressions.singleSwimmerPatternSecondary).Success)
                {
                    var regex = new Regex(RegexExpressions.singleSwimmerPatternSecondary);
                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        var laneInfo = new LaneInfo
                        {
                            LaneNumber = int.TryParse(match.Groups["laneNumber"].Value.Trim(), out int ln) ? ln : 0,
                            TeamName = match.Groups["teamName"].Value.Trim(),
                            SeedTime = match.Groups["seedTime"].Value.Trim(),
                            SwimmerName = SwapLastCommaFirstToFirstLast(match.Groups["name"].Value.Trim()),
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
                // Relay swimmer line
                else if (Regex.Match(line, RegexExpressions.relaySwimmerPattern).Success)
                {
                    ParseRelaySwimmers(line, currentHeat, RegexExpressions.relaySwimmerPattern);
                }
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
            return swimMeet;
        }

        private static string SwapLastCommaFirstToFirstLast(string name)
        {
            var names = name.Split(',');
            name = names[1].Trim() + " " + names[0].Trim();
            return name;
        }
        internal static Tuple<string, string, string, bool> ParseRelaySwimmerEventInfo(string line, string pattern)
        {
            Regex regex = new(pattern);
            Match match = regex.Match(line);

            string seedTime = match.Groups["seedTime"].Value.Trim();
            string teamName = match.Groups["teamName"].Value.Trim(); ;
            string laneNumber = match.Groups["laneNumber"].Value.Trim(); ;
            var infoToReturn = new Tuple<string, string, string, bool>(seedTime, teamName, laneNumber, false);
            return infoToReturn;
        }
        internal static void ParseRelaySwimmers(string line, HeatInfo currentHeat, string relayPattern)
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
                LaneNumber = int.TryParse(match.Groups["laneNumber"].Value.Trim(), out int ln) ? ln : 0,
                TeamName = match.Groups["teamName"].Value.Trim(),
                RelayTeamLetter = match.Groups["relayTeamLetter"].Value.Trim(),
                SeedTime = match.Groups["seedTime"].Value.Trim(),
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
        //internal static Tuple<string, string, string, bool> ParseRelaySwimmers(string line, Tuple<string, string, string, bool> relayInfo, string relayPattern)
        //{
        //    int heatId;
        //    string commandText;
        //    try
        //    {
        //        commandText = "SELECT heatId FROM Heat ORDER BY heatId DESC";
        //        heatId = App.InMemoryConnection.ExecuteScalar<int>(commandText, commandType: CommandType.Text);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString(), ConsoleColor.Red);
        //        return null;
        //    }
        //    Regex regex = new(relayPattern);
        //    Match match = regex.Match(line);

        //    var firstName = match.Groups["name1"].Value.Trim();
        //    if (firstName.Contains(','))
        //    {
        //        firstName = SwapLastCommaFirstToFirstLast(firstName);
        //    }
        //    var secondName = match.Groups["name2"].Value.Trim();
        //    if (secondName.Contains(','))
        //    {
        //        secondName = SwapLastCommaFirstToFirstLast(secondName);
        //    }

        //    try
        //    {
        //        var parameters = new DynamicParameters();
        //        parameters.Add("@heatId", heatId);
        //        parameters.Add("@name1", firstName);
        //        parameters.Add("@name2", secondName);
        //        parameters.Add("@age1", match.Groups["age1"].Value.Trim());
        //        parameters.Add("@age2", match.Groups["age2"].Value.Trim());
        //        parameters.Add("@seedTime", relayInfo.Item1);
        //        parameters.Add("@teamName", relayInfo.Item2);
        //        parameters.Add("@laneNumber", relayInfo.Item3);

        //        commandText = "INSERT INTO Swimmer (heatId, name, age, laneNumber, teamName, seedTime) ";
        //        commandText += "VALUES (@heatId, @name1, @age1, @laneNumber, @teamName, @seedTime)";

        //        //App.InMemoryConnection.Query(commandText, param: parameters, commandType: CommandType.Text);

        //        commandText = "INSERT INTO Swimmer (heatId, name, age, laneNumber, teamName, seedTime) ";
        //        commandText += "VALUES (@heatId, @name2, @age2, @laneNumber, @teamName, @seedTime)";

        //        //App.InMemoryConnection.Query(commandText, param: parameters, commandType: CommandType.Text);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString(), ConsoleColor.Blue);
        //        return null;
        //    }

        //    if (relayInfo.Item4 == false)
        //    {
        //        return new Tuple<string, string, string, bool>(relayInfo.Item1, relayInfo.Item2, relayInfo.Item3, true);
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}
        
        private static string CleanseTheData(string line)
        {
            return Regex.Replace(line, @"Β", "F").ToUpper(); //@"[^\d\s\w,:\-.'\/\!\@\#\$\%\^\&\*\(\)]", "?");
        }
 
        //internal static void FillEmptyTimes()
        //{
        //    string commandText = "SELECT heatId FROM Heat h WHERE h.startTime = ''";

        //    List<string> heatsWithoutStartTime = App.InMemoryConnection.Query<string>(commandText).ToList();

        //    foreach (var heat in heatsWithoutStartTime)
        //    {
        //        //Do SQL to get these
        //        string prevEventTime, nextEventTime;
        //        var parameter = new DynamicParameters();
        //        parameter.Add("@heatId", heat);
        //        commandText = "SELECT startTime FROM Heat h WHERE h.heatId = @heatId - 1;";
        //        prevEventTime = App.InMemoryConnection.Query<string>(commandText, param: parameter).FirstOrDefault();
        //        if (prevEventTime == null) { return; }

        //        commandText = "SELECT startTime FROM Heat h WHERE h.heatId = @heatId + 1;";
        //        nextEventTime = App.InMemoryConnection.Query<string>(commandText, param: parameter).FirstOrDefault();
        //        if (nextEventTime == null) { return; }

        //        if (prevEventTime == "" || nextEventTime == "") { continue; }

        //        DateTime prevDateTime = DateTime.Parse(prevEventTime);
        //        DateTime nextDateTime = DateTime.Parse(nextEventTime);

        //        TimeSpan timeBetween = nextDateTime - prevDateTime;
        //        TimeSpan newTime = new(0, (timeBetween.Minutes / 2), 0);
        //        DateTime newDateTime = prevDateTime + newTime;

        //        parameter.Add("@startTime", newDateTime.ToString("t"));
        //        commandText = "UPDATE Heat ";
        //        commandText += "SET startTime = @startTime ";
        //        commandText += "WHERE heatId = @heatId;";
        //        App.InMemoryConnection.Execute(commandText, param: parameter);
        //    }
        //}

    }
}
