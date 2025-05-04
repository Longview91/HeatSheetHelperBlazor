using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace HeatSheetHelper.Model
{
    public partial class Event : ObservableObject
    {        //Like the AnimalGroup class in the example
        [ObservableProperty]
        public string eventInfo;
        [ObservableProperty]
        public ObservableCollection<HeatInfo> heatsList;
        public Event(string oneEventInfo, ObservableCollection<HeatInfo> heats)
        {
            EventInfo = oneEventInfo;
            HeatsList = heats;
        }
    }
}
