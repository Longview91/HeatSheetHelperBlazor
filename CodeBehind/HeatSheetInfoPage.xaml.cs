//using HeatSheetHelper.Helpers;
//using HeatSheetHelper.Model;

//namespace HeatSheetHelper.Pages;

//public partial class HeatSheetInfo : ContentPage
//{
//    public HeatSheetInfo(List<HeatInfo> heatInfo)
//	{
//        InitializeComponent();
//        heatInfo = heatInfo.OrderBy(l => l.LaneNum).OrderBy(h => h.HeatNum).OrderBy(e => e.EventNum).ToList<HeatInfo>();
//        HeatInfoListView.ItemsSource = heatInfo;
//        eventToCountFrom.ItemsSource = DbHelper.PopulateEventPicker(App.InMemoryConnection);
//        titleText.Text = "  Event " + heatInfo[0].EventNum + "  Heat " + heatInfo[0].HeatNum;
//    }
//    private async void ReturnToMain(object sender, EventArgs e)
//    {
//        await Navigation.PopAsync();
//    }

//    private async void eventToCountFrom_SelectedIndexChanged(object sender, EventArgs e)
//    {
//        try
//        {
//            Picker eventPicker = (Picker)sender;
//            var eventNum = eventPicker.SelectedItem.ToString();
//            heatToCountFrom.ItemsSource = await DbHelper.PopulateHeatPicker(App.InMemoryConnection, eventNum);
//            heatToCountFrom.SelectedIndex = 0;
//        }
//        catch (Exception ex)
//        {
//            await DisplayAlert("Error", "Error occurred getting heat numbers: " + ex.Message, "OK");
//        }
//    }
//    private async void heatToCountFrom_SelectedIndexChanged(object sender, EventArgs e)
//    {
//        try
//        {
//            if (heatToCountFrom.SelectedIndex >= 0)
//            {
//                var eventFrom = eventToCountFrom.SelectedItem.ToString();
//                var heatFrom = heatToCountFrom.SelectedItem.ToString();
//                var heatInfo = (List<HeatInfo>)HeatInfoListView.ItemsSource;
//                var eventTo = heatInfo[0].EventNum;
//                var heatTo = heatInfo[0].HeatNum;
//                HeatCountLabel.Text = await DbHelper.CountHeats(App.InMemoryConnection, eventFrom, heatFrom, eventTo.ToString(), heatTo.ToString());
//            }
//        }
//        catch (Exception ex)
//        {
//            await DisplayAlert("Error", "Error occurred counting events: " + ex.Message, "OK");
//        }
//    }
//}