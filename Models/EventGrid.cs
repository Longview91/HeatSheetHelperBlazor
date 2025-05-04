using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HeatSheetHelper.Model
{
    public partial class EventGrid : INotifyPropertyChanged
    {
        private string _swimmerName;
        private string _eventNum;
        private string _heatNum;
        private int _laneNum;
        private string _eventName;
        private string _eventTime;
        private string _pBTime;
        private string _heatCount;
        private bool _isRelay = false;

        public string SwimmerName
        {
            get { return _swimmerName; }
            set { SetProperty(ref _swimmerName, value); }
        }
        public string EventNum
        {
            get { return _eventNum; }
            set { SetProperty(ref _eventNum, value); }
        }
        public string HeatNum
        {
            get { return _heatNum; }
            set { SetProperty(ref _heatNum, value); }
        }
        public int LaneNum
        {
            get { return _laneNum; }
            set { SetProperty(ref _laneNum, value); }
        }
        public string EventName
        {
            get { return _eventName; }
            set { SetProperty(ref _eventName, value); }
        }
        public string EventTime
        {
            get { return _eventTime; }
            set { SetProperty(ref _eventTime, value); }
        }
        public string PBTime
        {
            get { return _pBTime; }
            set { SetProperty(ref _pBTime, value); }
        }
        public string HeatCount
        {
            get { return _heatCount; }
            set { SetProperty(ref _heatCount, value); }
        }
        public bool IsRelay
        {
            get { return _isRelay; }
            set { _isRelay = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;

            backingField = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public bool Equals(EventGrid eventInfo)
        {
            return EventNum == eventInfo.EventNum &&
                    HeatNum == eventInfo.HeatNum &&
                    LaneNum == eventInfo.LaneNum;
        }

        public class EventGridComparer : IComparer<EventGrid>
        {
            public int Compare(EventGrid x, EventGrid y)
            {
                // Compare by EventNum first
                int eventComparison = int.Parse(x.EventNum).CompareTo(int.Parse(y.EventNum));
                if (eventComparison != 0)
                {
                    return eventComparison;
                }

                // If EventNum is equal, compare by HeatNum
                int heatComparison = int.Parse(x.HeatNum).CompareTo(int.Parse(y.HeatNum));
                if (heatComparison != 0)
                {
                    return heatComparison;
                }

                // If both EventNum and HeatNum are equal, compare by LaneNum
                return x.LaneNum.CompareTo(y.LaneNum);
            }
        }
    }
}
