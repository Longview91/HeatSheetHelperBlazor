using CommunityToolkit.Mvvm.ComponentModel;

namespace HeatSheetHelper.Model
{
    public partial class HeatInfo : ObservableObject
    {
        //Like the Animal class in the example
        [ObservableProperty]
        public int laneNum;
        [ObservableProperty]
        public string swimmerName;
        [ObservableProperty]
        public string age;
        [ObservableProperty]
        public string teamName;
        [ObservableProperty]
        public string seedTime;
        [ObservableProperty]
        public string eventInfo;
        private int _eventNum;
        private int _heatNum;
        private string _startTime;
        private bool _isRelay = false;

        //public string LaneNum
        //{
        //    get { return _laneNum; }
        //    set { SetEventProperty(ref _laneNum, value); }
        //}
        //public string SwimmerName
        //{
        //    get { return _swimmerName; }
        //    set { SetEventProperty(ref _swimmerName, value); }
        //}
        //public string Age
        //{
        //    get { return _age; }
        //    set { SetEventProperty(ref _age, value); }
        //}
        //public string TeamName
        //{
        //    get { return _teamName; }
        //    set { SetEventProperty(ref _teamName, value); }
        //}
        //public string SeedTime
        //{
        //    get { return _seedTime; }
        //    set { SetEventProperty(ref _seedTime, value); }
        //}
        public int EventNum
        {
            get { return _eventNum; }
            set { _eventNum = value; }
        }
        public int HeatNum
        {
            get { return _heatNum; }
            set { _heatNum = value; }
        }
        public string StartTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }
        public bool IsRelay
        {
            get { return _isRelay; }
            set { _isRelay = value; }
        }
        //public bool SetEventProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action onChanged = null)
        //{
        //    if (EqualityComparer<T>.Default.Equals(backingStore, value))
        //        return false;

        //    backingStore = value;
        //    onChanged?.Invoke();
        //    OnPropertyChanged(propertyName);
        //    return true;
        //}
        //public event PropertyChangedEventHandler PropertyChanged;
        //public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        //{
        //    if (PropertyChanged != null)
        //        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        //}

    }
}
