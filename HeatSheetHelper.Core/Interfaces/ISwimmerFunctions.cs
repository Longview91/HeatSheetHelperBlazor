using HeatSheetHelper.Core.Shared;

namespace HeatSheetHelper.Core.Interfaces
{
    public interface ISwimmerFunctions
    {
        string CleanseTheData(string line);
        void FillMissingHeatStartTimes(SwimMeet swimMeet);
        SwimMeet ParseHeatSheetToEvents(List<string> heatSheet);
        Tuple<string, string, string, bool> ParseRelaySwimmerEventInfo(string line, string pattern);
        void ParseRelaySwimmers(string line, HeatInfo currentHeat, int currentRelayLane, string currentRelayTeamName, string currentRelayTeamLetter, string currentRelaySeedTime, string relayPattern);
        string SwapLastCommaFirstToFirstLast(string name);
        bool TryParseTime(string time, out DateTime dt);
    }
}