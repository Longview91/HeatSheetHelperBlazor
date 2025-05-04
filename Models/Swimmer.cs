namespace HeatSheetHelper.Model
{
    internal class Swimmer
    {
        private int _eventNumber = 0; public int EventNumber { get { return _eventNumber; } set { _eventNumber = value; } }
        private int _distance = 0; public int Distance { get { return _distance; } set { _distance = value; } }
        private string _courseType = ""; public string CourseType { get { return _courseType; } set { _courseType = value; } }
        private string _unitType = ""; public string UnitType { get { return _unitType; } set { _unitType = value; } }
        private string _strokeType = ""; public string StrokeType { get { return _strokeType; } set { _strokeType = value; } }
        private bool _isRelay = false; public bool IsRelay { get { return _isRelay; } set { _isRelay = value; } }
        private int _eventName = 0; public int EventName { get { return _eventName; } set { _eventName = value; } }
        private int _heatNumber = 0; public int HeatNumber { get { return _heatNumber; } set { _heatNumber = value; } }
        private string _startTime = ""; public string StartTime { get { return _startTime; } set { _startTime = value; } }
        private int _swimmerId = 0; public int SwimmerId { get { return _swimmerId; } set { _swimmerId = value; } }
        private int _heatId = 0; public int HeatId { get { return _heatId; } set { _heatId = value; } }
        private string _laneNumber = "0"; public string LaneNumber { get { return _laneNumber; } set { _laneNumber = value; } }
        private string _name = ""; public string Name { get { return _name; } set { _name = value; } }
        private string _age = ""; public string Age { get { return _age; } set { _age = value; } }
        private string _teamName = ""; public string TeamName { get { return _teamName; } set { _teamName = value; } }
        private string _pbTime = ""; public string PBTime { get { return _pbTime; } set { _pbTime = value; } }

        internal void Copy(Swimmer currSwimmer)
        {
            EventNumber = currSwimmer.EventNumber;
            Distance = currSwimmer.Distance;
            CourseType = currSwimmer.CourseType;
            UnitType = currSwimmer.UnitType;
            StrokeType = currSwimmer.StrokeType;
            IsRelay = currSwimmer.IsRelay;
            EventName = currSwimmer.EventName;
            HeatNumber = currSwimmer.HeatNumber;
            StartTime = currSwimmer.StartTime;
            SwimmerId = currSwimmer.SwimmerId;
            HeatId = currSwimmer.HeatId;
            LaneNumber = currSwimmer.LaneNumber;
            Name = currSwimmer.Name;
            Age = currSwimmer.Age;
            TeamName = currSwimmer.TeamName;
            PBTime = currSwimmer.PBTime;
        }
    }
}
