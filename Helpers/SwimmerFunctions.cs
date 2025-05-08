using Dapper;
using HeatSheetHelper.Constants;
using HeatSheetHelper.Model;
using HeatSheetHelperBlazor;
using System.Data;
using System.Text.RegularExpressions;

namespace HeatSheetHelper.Helpers
{
    internal class SwimmerFunctions
    {

        /// <summary>
        /// 
        /// </summary>
        internal static List<Swimmer> PutHeatSheetInClasses(List<string> heatSheet)
        {
            List<Swimmer> swimmersToReturn = new();

            bool isAlternate = false;
            Tuple<string, string, string, bool> relayInfo = null;
            Swimmer swimmer = new Swimmer();

            foreach (var dirtyLine in heatSheet)
            {
                string line = CleanseTheData(dirtyLine);
                if (line.StartsWith("ALTERNATE"))
                {
                    isAlternate = true;
                }
                if (line.StartsWith('#') || line.StartsWith("EVENT") || Regex.Match(line, @"\bHEAT\s+(\d+)\s?[A-Z]?\s").Success || Regex.Match(line, @"HEAT +\d+ +\(").Success || Regex.Match(line, @"([A-Z]+ )((\d+:)?\d{2}\.\d{2}|X?NT)([A-Z]+-[A-Z]+)\d").Success)
                {
                    //reset the relay info if we find a new event, heat or team line
                    relayInfo = null;
                }

                if (relayInfo == null)
                {
                    FindPatternThatMatches(ref relayInfo, ref isAlternate, line, swimmer, swimmersToReturn);
                }
                else if (Regex.Match(line, RegexExpressions.relaySwimmerPattern).Success) //Relay event
                {
                    try
                    {
                        relayInfo = SwimmerFunctions.ParseRelaySwimmers(line, relayInfo, RegexExpressions.relaySwimmerPattern);
                    }
                    catch (Exception)
                    {
                        relayInfo = null;
                    }
                }
            }
            return swimmersToReturn;
        }

        private static Swimmer? FindPatternThatMatches(ref Tuple<string, string, string, bool> relayInfo, ref bool isAlternate, string line, Swimmer swimmer, List<Swimmer> swimmersToReturn)
        {
            if (line.Contains("HY-TEK'S ")) { return null; } //Skip this header line for improved efficiency in the regex

            if (line.StartsWith('#') || line.StartsWith("EVENT")) //This is an event line so we will set the event info
            {
                isAlternate = false;
                swimmer = SwimmerFunctions.ParseEventLine(line);
            }
            else if (Regex.Match(line, @"\bHEAT\s+(\d+)\s?[A-Z]?\s").Success) //This is a heat line
            {
                isAlternate = false;
                swimmer = SwimmerFunctions.ParseHeatLine(line, swimmer);
            }
            else if (Regex.Match(line, @"HEAT +\d+ +\(").Success) //This is a heat line
            {
                isAlternate = false;
                swimmer = SwimmerFunctions.ParseHeatLineOnNewPage(line, swimmer);
            }
            else if (Regex.Match(line, RegexExpressions.singleSwimmerPatternSecondary).Success)
            {
                swimmer = SwimmerFunctions.ParseIndividualEvent(line, RegexExpressions.singleSwimmerPatternSecondary, isAlternate, swimmer);
                swimmersToReturn.Add(swimmer);
                swimmer.Name = string.Empty;
                swimmer.Age = string.Empty;
                swimmer.TeamName = string.Empty;
                swimmer.PBTime = string.Empty;
                swimmer.LaneNumber = string.Empty;
            }
            else if (Regex.Match(line, RegexExpressions.singleSwimmerPatternMain).Success)
            {
                swimmer = SwimmerFunctions.ParseIndividualEvent(line, RegexExpressions.singleSwimmerPatternMain, isAlternate, swimmer);
                swimmersToReturn.Add(swimmer);
                swimmer.Name = string.Empty;
                swimmer.Age = string.Empty;
                swimmer.TeamName = string.Empty;
                swimmer.PBTime = string.Empty;
                swimmer.LaneNumber = string.Empty;
            }
            else if (Regex.Match(line, RegexExpressions.relayTeamPattern).Success) //Relay event
            {
                try
                {
                    relayInfo = SwimmerFunctions.ParseRelaySwimmerEventInfo(line, RegexExpressions.relayTeamPattern);
                }
                catch (Exception)
                {
                    relayInfo = null;
                }
            }
            return swimmer;
        }

        private static string SwapLastCommaFirstToFirstLast(string name)
        {
            var names = name.Split(',');
            name = names[1].Trim() + " " + names[0].Trim();
            return name;
        }
        internal static Swimmer ParseEventLine(string line)
        {
            // Extract the event number (first number in the line)
            int eventNumber = int.Parse(Regex.Match(line, @"\d+").Value);

            // Treat the rest of the line (after the event number) as the event name
            string eventName = line.Substring(line.IndexOf(' ') + 1).Trim();

            Swimmer current = new Swimmer();
            current.EventNumber = eventNumber;
            current.EventName = eventName;

            return current;
        }
        internal static Swimmer ParseHeatLine(string line, Swimmer swimmer)
        {
            swimmer.HeatNumber = int.Parse(Regex.Match(line, @"(?<=HEAT +)\d{1,}").Value.Trim());
            swimmer.StartTime = Regex.Match(line, @"([1-9]|10|11|12):\d{2}\s*(AM|PM)").Value.Trim();

            return swimmer;
        }
        internal static Swimmer ParseHeatLineOnNewPage(string line, Swimmer swimmer)
        {
            swimmer.HeatNumber = int.Parse(Regex.Match(line, @"(?<=HEAT +)\d{1,}").Value.Trim());
            swimmer.StartTime = String.Empty;

            return swimmer;
        }
        internal static Swimmer ParseIndividualEvent(string line, string pattern, bool isAlternate, Swimmer swimmer)
        {
            Regex regex = new(pattern);
            Match match = regex.Match(line);

            string name = match.Groups["name"].Value.Trim();

            if (name.Contains(','))
            {
                name = SwapLastCommaFirstToFirstLast(name);
            }

            swimmer.Name = name;
            swimmer.Age = match.Groups["age"].Value.Trim();
            if (isAlternate == true)
            {
                swimmer.LaneNumber = "Alt" + match.Groups["laneNumber"].Value.Trim();
            }
            else
            {
                swimmer.LaneNumber = match.Groups["laneNumber"].Value.Trim();
            }
            swimmer.TeamName = match.Groups["teamName"].Value.Trim();
            swimmer.PBTime = match.Groups["seedTime"].Value.Trim();

            return swimmer;
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
        internal static Tuple<string, string, string, bool> ParseRelaySwimmers(string line, Tuple<string, string, string, bool> relayInfo, string relayPattern)
        {
            int heatId;
            string commandText;
            try
            {
                commandText = "SELECT heatId FROM Heat ORDER BY heatId DESC";
                heatId = App.InMemoryConnection.ExecuteScalar<int>(commandText, commandType: CommandType.Text);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString(), ConsoleColor.Red);
                return null;
            }
            Regex regex = new(relayPattern);
            Match match = regex.Match(line);

            var firstName = match.Groups["name1"].Value.Trim();
            if (firstName.Contains(','))
            {
                firstName = SwapLastCommaFirstToFirstLast(firstName);
            }
            var secondName = match.Groups["name2"].Value.Trim();
            if (secondName.Contains(','))
            {
                secondName = SwapLastCommaFirstToFirstLast(secondName);
            }

            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@heatId", heatId);
                parameters.Add("@name1", firstName);
                parameters.Add("@name2", secondName);
                parameters.Add("@age1", match.Groups["age1"].Value.Trim());
                parameters.Add("@age2", match.Groups["age2"].Value.Trim());
                parameters.Add("@seedTime", relayInfo.Item1);
                parameters.Add("@teamName", relayInfo.Item2);
                parameters.Add("@laneNumber", relayInfo.Item3);

                commandText = "INSERT INTO Swimmer (heatId, name, age, laneNumber, teamName, seedTime) ";
                commandText += "VALUES (@heatId, @name1, @age1, @laneNumber, @teamName, @seedTime)";

                App.InMemoryConnection.Query(commandText, param: parameters, commandType: CommandType.Text);

                commandText = "INSERT INTO Swimmer (heatId, name, age, laneNumber, teamName, seedTime) ";
                commandText += "VALUES (@heatId, @name2, @age2, @laneNumber, @teamName, @seedTime)";

                App.InMemoryConnection.Query(commandText, param: parameters, commandType: CommandType.Text);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString(), ConsoleColor.Blue);
                return null;
            }

            if (relayInfo.Item4 == false)
            {
                return new Tuple<string, string, string, bool>(relayInfo.Item1, relayInfo.Item2, relayInfo.Item3, true);
            }
            else
            {
                return null;
            }
        }
        internal static void ShortenEventName(ref string eventName, string distance, bool isRelay)
        {
            string relay;
            string shortEvent;
            if (!isRelay)
            {
                if (eventName.Contains("FREESTYLE"))
                {
                    shortEvent = "FR";
                }
                else if (eventName.Contains("BUTTER")) //Weird situation where the f is a Beta character
                {
                    shortEvent = "FL";
                }
                else if (eventName.Contains("BACKSTROKE"))
                {
                    shortEvent = "BA";
                }
                else if (eventName.Contains("BREASTSTROKE"))
                {
                    shortEvent = "BR";
                }
                else { shortEvent = "IM"; }
                relay = "";
            }
            else
            {
                if (eventName.Contains("FREESTYLE"))
                {
                    shortEvent = "FR";
                }
                else if (eventName.Contains("BUTTER")) //Weird situation where the f is a Beta character
                {
                    shortEvent = "FL";
                }
                else if (eventName.Contains("BACKSTROKE"))
                {
                    shortEvent = "BA";
                }
                else if (eventName.Contains("BREASTSTROKE"))
                {
                    shortEvent = "BR";
                }
                else { shortEvent = "MD"; }
                relay = "RELAY";
            }
            eventName = (distance + " " + shortEvent + " " + relay).Trim();
        }
        private static string CleanseTheData(string line)
        {
            return Regex.Replace(line, @"Β", "F"); //@"[^\d\s\w,:\-.'\/\!\@\#\$\%\^\&\*\(\)]", "?");
        }
        internal static void FillEmptyTimes()
        {
            string commandText = "SELECT heatId FROM Heat h WHERE h.startTime = ''";

            List<string> heatsWithoutStartTime = App.InMemoryConnection.Query<string>(commandText).ToList();

            foreach (var heat in heatsWithoutStartTime)
            {
                //Do SQL to get these
                string prevEventTime, nextEventTime;
                var parameter = new DynamicParameters();
                parameter.Add("@heatId", heat);
                commandText = "SELECT startTime FROM Heat h WHERE h.heatId = @heatId - 1;";
                prevEventTime = App.InMemoryConnection.Query<string>(commandText, param: parameter).FirstOrDefault();
                if (prevEventTime == null) { return; }

                commandText = "SELECT startTime FROM Heat h WHERE h.heatId = @heatId + 1;";
                nextEventTime = App.InMemoryConnection.Query<string>(commandText, param: parameter).FirstOrDefault();
                if (nextEventTime == null) { return; }

                if (prevEventTime == "" || nextEventTime == "") { continue; }

                DateTime prevDateTime = DateTime.Parse(prevEventTime);
                DateTime nextDateTime = DateTime.Parse(nextEventTime);

                TimeSpan timeBetween = nextDateTime - prevDateTime;
                TimeSpan newTime = new(0, (timeBetween.Minutes / 2), 0);
                DateTime newDateTime = prevDateTime + newTime;

                parameter.Add("@startTime", newDateTime.ToString("t"));
                commandText = "UPDATE Heat ";
                commandText += "SET startTime = @startTime ";
                commandText += "WHERE heatId = @heatId;";
                App.InMemoryConnection.Execute(commandText, param: parameter);
            }
        }

    }
}
