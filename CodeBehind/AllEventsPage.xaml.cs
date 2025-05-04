//using CommunityToolkit.Maui.Core.Extensions;
//using Dapper;
//using HeatSheetHelper.Helpers;
//using HeatSheetHelper.Model;
//using HeatSheetHelper.ViewModel;
//using System.Collections.ObjectModel;

//namespace HeatSheetHelper.Pages;

//public partial class AllEvents : ContentPage
//{
//    public EventViewModel eventsToBeBound = new EventViewModel();
//    private ObservableCollection<HeatInfo> heatInfos = new();
//    private ObservableCollection<HeatInfo> teamsSelected = new();
//    public AllEvents(ObservableCollection<HeatInfo> heats)
//    {
//        try
//        {
//            heatInfos = heats;
//            InitializeComponent();
//            eventToScrollTo.ItemsSource = DbHelper.PopulateEventPicker(App.InMemoryConnection);
//            _ = LoadData(heatInfos);
//            BindingContext = eventsToBeBound;
//        }
//        catch
//        {
//            DisplayAlert("Error", "Error occurred in AllEvents constructor", "OK");
//        }
//    }
//    private async Task LoadData(ObservableCollection<HeatInfo> heats)
//    {
//        try
//        {
//            List<string> teamNames = new() { "" };
//            foreach (HeatInfo heat in heats)
//            {
//                if (!teamNames.Contains(heat.TeamName))
//                {
//                    teamNames.Add(heat.TeamName);
//                }
//            }
//            teamNames.Sort();
//            teamPicker.ItemsSource = teamNames;
//            await eventsToBeBound.CreateEventsCollectionAsync(heats);
//        }
//        catch
//        {
//            await DisplayAlert("Error", "Error occurred in AllEvents while loading data", "OK");
//        }
//    }
//    private async void ReturnToMain(object sender, EventArgs e)
//    {
//        await Navigation.PopAsync();
//    }

//    private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
//    {
//        var myListView = (ListView)sender;
//        if (myListView.SelectedItem == null)
//            return;
//        var newSelectedItem = e.Item as HeatInfo;

//        var parameter = new DynamicParameters();
//        parameter.Add("@eventNum", newSelectedItem.EventNum);
//        parameter.Add("@heatNum", newSelectedItem.HeatNum);

//        string commandText = "SELECT e.eventNumber AS EventNum, h.heatNumber AS HeatNum, s.laneNumber AS LaneNum, GROUP_CONCAT(s.name, '\r\n') AS SwimmerName, GROUP_CONCAT(s.age, '\r\n') AS Age, s.teamName as TeamName, s.seedTime AS SeedTime FROM Swimmer s ";
//        commandText += "JOIN Heat h ON h.heatId = s.heatId ";
//        commandText += "JOIN Event e ON e.eventId = h.eventId ";
//        commandText += "WHERE eventNum = @eventNum AND heatNum = @heatNum ";
//        commandText += "GROUP BY e.eventNumber, h.heatNumber, s.laneNumber, s.teamName, s.seedTime ";
//        commandText += "ORDER BY eventNumber ASC, heatNumber ASC, laneNumber ASC;";

//        var heats = App.InMemoryConnection.Query<HeatInfo>(commandText, param: parameter).ToList();

//        await Navigation.PushAsync(new HeatSheetInfo(heats));

//        myListView.SelectedItem = null;
//    }

//    private void myViewCell_HandlerChanged(object sender, EventArgs e)
//    {
//        var myViewCell = (CustomViewCell)sender;
//        if (myViewCell.SelectedBackgroundColor.Equals(Color.Parse("#BB0000")))
//        {
//            myViewCell.SelectedBackgroundColor = Color.Parse("White");
//        }
//    }

//    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
//    {
//        try
//        {
//            EventViewModel myModel = BindingContext as EventViewModel;
//            int group = myModel.Events.IndexOf(myModel.Events.FirstOrDefault(e => e.EventInfo.StartsWith("Event " + eventToScrollTo.SelectedItem.ToString() + "  Heat " + heatToScrollTo.SelectedItem.ToString())));
//            EventList.ScrollTo(group,-1,ScrollToPosition.Start,false);
//        }
//        catch
//        {
//            await DisplayAlert("Error", "Error occurred scrolling to the event and heat", "OK");
//        }
//    }
//    private async void eventToScrollTo_SelectedIndexChanged(object sender, EventArgs e)
//    {
//        try
//        {
//            Picker eventPicker = (Picker)sender;
//            List<string> newHeatNumList = new();
//            if (eventPicker.SelectedItem == null)
//            {
//                heatToScrollTo.ItemsSource = newHeatNumList;
//                return;
//            }
//            var eventNum = eventPicker.SelectedItem.ToString();
//            foreach (var events in eventsToBeBound.Events)
//            {
//                foreach (var myEvent in events.HeatsList)
//                {
//                    if (int.Parse(eventNum) == myEvent.EventNum && !newHeatNumList.Contains(myEvent.HeatNum.ToString()))
//                    {
//                        newHeatNumList.Add(myEvent.HeatNum.ToString());
//                    }
//                }
//            }
//            heatToScrollTo.ItemsSource = newHeatNumList;
//            //heatToScrollTo.ItemsSource = await DbHelper.PopulateHeatPicker(App.InMemoryConnection, eventNum);
//            heatToScrollTo.SelectedIndex = 0;
//        }
//        catch
//        {
//            await DisplayAlert("Error", "Error occurred getting heat numbers", "OK");
//        }
//    }
//    private async void teamChosen(object sender, EventArgs e)
//    {
//        try
//        {
//            Picker teamPicker = (Picker)sender;
//            if (teamPicker.SelectedItem.ToString() == "")
//            {
//                eventToScrollTo.ItemsSource = DbHelper.PopulateEventPicker(App.InMemoryConnection);
//                eventsToBeBound = new EventViewModel();
//                await eventsToBeBound.CreateEventsCollectionAsync(heatInfos);
//                BindingContext = eventsToBeBound;
//                return;
//            }
//            List<HeatInfo> newHeatList = new List<HeatInfo>();
//            List<string> newEventList = new List<string>();
//            foreach (var heat in heatInfos)
//            {
//                if (heat.TeamName == teamPicker.SelectedItem.ToString())
//                {
//                    newHeatList.Add(heat);
//                    if (!newEventList.Contains(heat.EventNum.ToString()))
//                    {
//                        newEventList.Add(heat.EventNum.ToString());
//                    }
//                }
//            }
//            eventToScrollTo.ItemsSource = newEventList;
//            ObservableCollection<HeatInfo> teamsSelected = newHeatList.ToObservableCollection();
//            eventsToBeBound = new EventViewModel();
//            await eventsToBeBound.CreateEventsCollectionAsync(teamsSelected);
//            BindingContext = eventsToBeBound;
//        }
//        catch
//        {
//            await DisplayAlert("Error", "Error occurred with the teamPicker", "OK");
//        }
//    }
//}