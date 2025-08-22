using HeatSheetHelper.Core.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HeatSheetHelper.Core.States
{
    internal class StateFactory
    {
        public State CreateState(string line)
        {
            // Event line
            if (line.StartsWith('#') || line.StartsWith("EVENT"))
            {
                return new EventState();
            }
            // Heat line
            else if (Regex.Match(line, @"\bHEAT\s+(\d+)").Success)
            {
                return new HeatState();
            }
            // Lane/Swimmer line (individual)
            else if (Regex.Match(line, RegexExpressions.singleSwimmerPatternMain).Success)
            {
                return new LanePattern1State();
            }
            // Lane/Swimmer line (secondary pattern)
            else if (Regex.Match(line, RegexExpressions.singleSwimmerPatternSecondary).Success)
            {
                return new LanePattern2State();
            }
            else if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
            {
                return new SkipLineState();
            }
            else
            {
                // Default state for unrecognized lines
                return new SkipLineState();
            }
        }
    }
}
