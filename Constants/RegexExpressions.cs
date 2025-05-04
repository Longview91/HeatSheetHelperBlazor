namespace HeatSheetHelper.Constants
{
    public static class RegexExpressions
    {
        public const string teamPattern = @"?<teamName>(([A-Z]+[- ]?[A-Z]+){0,5}|UN)";
        public const string singleSwimmerPatternMain = @"^(" + teamPattern + @")\s(?<seedTime>NT|XNT|(\d+:)?\d{1,2}\.\d{1,2}[LYS]?)[MW]?\s{0,2}(?<age>\d+|FR|SO|JR|SR)(?<name>[^\d]+)(?<laneNumber>\d{1,2})(?: _____)?";
        public const string singleSwimmerPatternSecondary = @"^(" + teamPattern + @")\s(?<seedTime>NT|XNT|(\d+:)?\d{1,2}\.\d{1,2}[LYS]?)(?<gender>[MW]?)\s{0,2}(?<age>\d+|FR|SO|JR|SR)(?<name>[^\d]+)(?:\w{2}\d{2})(?<laneNumber>\d{1,2})(?: _____)?";
        public const string relaySwimmerPattern = @"^(\d\) )?(?<name1>[A-Z][A-Z,\-\?\' ]+[A-Z\-])\s[WM]?(?<age1>(?:FR|SO|JR|SR|\d+))\s?(\d\) )?(?<name2>[A-Z][A-Z,\-\?\' ]+[A-Z\-])\s[WM]?(?<age2>(?:FR|SO|JR|SR|\d+))$";
        public const string relayTeamPattern = @"([A-Z]+ )(?<seedTime>(\d+:)?\d{2}\.\d{2}|X?NT)(?<teamName>([A-Z]+-[A-Z]+|UN)+)(?<laneNumber>(\d))";
    }
}
