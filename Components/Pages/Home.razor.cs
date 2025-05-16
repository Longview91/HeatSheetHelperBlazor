using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using System.Text.RegularExpressions;
using Dapper;
using HeatSheetHelper.Helpers;
using System.Collections.ObjectModel;
using HeatSheetHelper.Model;
using Microsoft.AspNetCore.Components;
using HeatSheetHelperBlazor;
using HeatSheetHelperBlazor.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Threading.Tasks;

namespace HeatSheetHelperBlazor.Components.Pages
{
    public partial class Home
    {
        private ErrorModal ErrorModal = new();
        private List<string> swimmerNameList = new();
        private List<Swimmer> swimmerHeats = new();
        private List<Swimmer> allSwimmers = new();

        public string? SelectedSwimmer { get; private set; }

        private async Task OnPDFPickClicked()
        {
            try
            {
                if (allSwimmers != null)
                {
                    allSwimmers.Clear();
                }
                else
                {
                    allSwimmers = new List<Swimmer>();
                }

                    var fileResult = await FilePicker.PickAsync(new PickOptions
                    {
                        PickerTitle = "Pick the heat sheet please",
                        FileTypes = FilePickerFileType.Pdf
                    });

                if (fileResult == null)
                    return;

                PdfDocument document = PdfDocument.Open(fileResult.FullPath);

                List<string> heatSheet = new();

                foreach (var page in document.GetPages())
                {
                    string pdfText = ContentOrderTextExtractor.GetText(page, true);
                    heatSheet.AddRange(Regex.Split(pdfText, "\n").ToList());
                }

                Tuple<string, string, string, bool> relayInfo = null;
                bool isAlternate = false;

                allSwimmers = SwimmerFunctions.PutHeatSheetInClasses(heatSheet);

                SwimmerFunctions.FillEmptyTimes();

                await CreateInitialList();

            }
            catch (Exception ex)
            {
                ErrorModal.Show("Error", "An error was encountered when loading the heat sheet: " + ex.Message);
            }
        }
        private async Task CreateInitialList()
        {
            swimmerNameList = await Task.Run(() =>
            {
                return allSwimmers
                .Select(s => s.Name)
                .Distinct()
                .OrderBy(name => name)
                .ToList();
            });

            StateHasChanged();
        }

        private async Task SwimmerPicker_SelectedIndexChanged(ChangeEventArgs e)
        {
            SelectedSwimmer = e.Value?.ToString();

            if (!string.IsNullOrEmpty(SelectedSwimmer))
            {
                // Fetch heats for the selected swimmer
                await LoadSwimmerHeats(SelectedSwimmer);
            }
        }

        private async Task LoadSwimmerHeats(string swimmerName)
        {
            try
            {
                swimmerHeats = await Task.Run(() =>
                {
                    return (from swimmer in allSwimmers
                            where swimmer.Name == swimmerName
                            select new Swimmer
                            {
                                EventNumber = swimmer.EventNumber,
                                HeatNumber = swimmer.HeatNumber,
                                EventName = swimmer.EventName,
                                StartTime = swimmer.StartTime,
                                LaneNumber = swimmer.LaneNumber
                            }).ToList();
                });

                // Update the UI (e.g., bind swimmerHeats to a grid or list)
                StateHasChanged();
            }
            catch (Exception ex)
            {
                ErrorModal.Show("Error", "An error was encountered when loading the swimmer heats: " + ex.Message);
            }
        }
        //private void AddToGrid(string swimmerName)
        //{
        //    if (selectedSwimmers.Count > 1)
        //    {
        //        TapHeatLabel.IsVisible = true;
        //        foreach (var swimEvent in eventGrid)
        //        {
        //            swimEvent.HeatCount = "";
        //        }
        //    }
        //    var parameter = new DynamicParameters();
        //    parameter.Add("@name", swimmerName);

        //    string commandText = "SELECT s.name AS SwimmerName, e.eventNumber AS EventNum, h.heatNumber AS HeatNum, s.laneNumber AS LaneNum, e.eventName as EventName, h.startTime AS EventTime, s.seedTime AS PBTime, e.isRelay AS IsRelay FROM Swimmer s ";
        //    commandText += "JOIN Heat h ON h.heatId = s.heatId ";
        //    commandText += "JOIN Event e ON e.eventId = h.eventId ";
        //    commandText += "WHERE s.name = @name ";
        //    commandText += "ORDER BY eventNumber ASC, heatNumber ASC, laneNumber ASC;";

        //    try
        //    {
        //        var heats = App.InMemoryConnection.Query<EventGrid>(commandText, param: parameter).ToList();

        //        foreach (var heat in heats)
        //        {
        //            if (heat.IsRelay)
        //            {
        //                commandText = "SELECT GROUP_CONCAT(s.name, ', ') FROM Swimmer s ";
        //                commandText += "JOIN Heat h ON h.heatId = s.heatId ";
        //                commandText += "JOIN Event e ON e.eventId = h.eventId ";
        //                commandText += "WHERE e.eventNumber = '" + heat.EventNum + "' AND h.heatNumber = '" + heat.HeatNum + "' AND s.laneNumber = '" + heat.LaneNum + "';";
        //                heat.SwimmerName = App.InMemoryConnection.Query<string>(commandText).FirstOrDefault();

        //                string pattern = @"^([^,]*, [^,]*), ";

        //                // Use Regex.Replace with a MatchEvaluator function
        //                heat.SwimmerName = Regex.Replace(heat.SwimmerName, pattern, ReplaceSecondComma);
        //            }
        //            int index = ObservableCollectionInsertIndex(eventGrid, heat);
        //            if (eventGrid.FirstOrDefault(e => e.SwimmerName.Contains(heat.SwimmerName) && e.EventNum == heat.EventNum && e.HeatNum == heat.HeatNum && e.LaneNum == heat.LaneNum) == null)
        //            {
        //                eventGrid.Insert(index < 0 ? ~index : index, heat);
        //            }
        //        }

        //        if (selectedSwimmers.Count == 1)
        //        {
        //            SetHeatCounts(eventGrid);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //    }
        //}
        //private void RemoveFromGrid(string swimmerName)
        //{
        //    var parameter = new DynamicParameters();
        //    parameter.Add("@name", swimmerName);

        //    string commandText = "SELECT s.name AS SwimmerName, e.eventNumber AS EventNum, h.heatNumber AS HeatNum, s.laneNumber AS LaneNum, e.eventName as EventName, h.startTime AS EventTime, s.seedTime AS PBTime FROM Swimmer s ";
        //    commandText += "JOIN Heat h ON h.heatId = s.heatId ";
        //    commandText += "JOIN Event e ON e.eventId = h.eventId ";
        //    commandText += "WHERE name = @name ";
        //    commandText += "GROUP BY e.eventNumber, h.heatNumber, s.laneNumber, e.eventName, h.startTime, s.seedTime ";
        //    commandText += "ORDER BY eventNumber ASC, heatNumber ASC, laneNumber ASC;";

        //    var heats = App.InMemoryConnection.Query<EventGrid>(commandText, param: parameter).ToList();
        //    foreach (var heat in heats)
        //    {
        //        eventGrid.Remove(eventGrid.FirstOrDefault(e => e.SwimmerName.Contains(heat.SwimmerName) && !selectedSwimmers.Any(s=> e.SwimmerName.Contains(s))));
        //    }

        //    if (selectedSwimmers.Count == 1)
        //    {
        //        SetHeatCounts(eventGrid);
        //    }
        //}
        //private static int ObservableCollectionInsertIndex(ObservableCollection<EventGrid> eventGrid, EventGrid heat)
        //{
        //    var eventGridComparer = new EventGrid.EventGridComparer();
        //    int insertIndex = eventGrid.ToList().BinarySearch(heat, eventGridComparer);
        //    return insertIndex;
        //}
        //private void SwimmerPicker_Focused(object sender, FocusEventArgs e)
        //{
        //    swimmerPicker.Title = "";
        //}
        //private async void OnGridItemTapped(object sender, SelectionChangedEventArgs e)
        //{
        //    if ((sender as CollectionView).SelectedItem == null)
        //        return;
        //    var newSelectedItem = e.CurrentSelection[0] as EventGrid;

        //    var parameter = new DynamicParameters();
        //    parameter.Add("@eventNum", newSelectedItem.EventNum);
        //    parameter.Add("@heatNum", newSelectedItem.HeatNum);

        //    string commandText = "SELECT e.eventNumber AS EventNum, h.heatNumber AS HeatNum, s.laneNumber AS LaneNum, GROUP_CONCAT(s.name, '\r\n') AS SwimmerName, GROUP_CONCAT(s.age, '\r\n') AS Age, s.teamName as TeamName, s.seedTime AS SeedTime FROM Swimmer s ";
        //    commandText += "JOIN Heat h ON h.heatId = s.heatId ";
        //    commandText += "JOIN Event e ON e.eventId = h.eventId ";
        //    commandText += "WHERE eventNum = @eventNum AND heatNum = @heatNum ";
        //    commandText += "GROUP BY e.eventNumber, h.heatNumber, s.laneNumber, s.teamName, s.seedTime ";
        //    commandText += "ORDER BY eventNumber ASC, heatNumber ASC, laneNumber ASC;";

        //    var heats = App.InMemoryConnection.Query<HeatInfo>(commandText, param: parameter).ToList();

        //    await Navigation.PushAsync(new HeatSheetInfo(heats));

        //    EventList.SelectedItem = null;
        //}
        //private static void SetHeatCounts(ObservableCollection<EventGrid> eventGrid)
        //{
        //    int prevHeatId = 0;
        //    EventGrid prevSwimEvent = null;
        //    foreach (var swimEvent in eventGrid)
        //    {
        //        var parameter = new DynamicParameters();
        //        parameter.Add("@heatNumber", swimEvent.HeatNum);
        //        parameter.Add("@eventNumber", swimEvent.EventNum);

        //        string commandText = "SELECT h.heatId FROM Heat h ";
        //        commandText += "JOIN Event e ON e.eventId = h.eventId ";
        //        commandText += "WHERE e.eventNumber = @eventNumber AND h.heatNumber = @heatNumber;";

        //        var currentHeatId = App.InMemoryConnection.Query<int>(commandText, param: parameter).FirstOrDefault();

        //        if (prevHeatId != 0)
        //        {
        //            prevSwimEvent.HeatCount = "Heats b/w: " + (currentHeatId - prevHeatId).ToString();
        //        }
        //        prevHeatId = currentHeatId;
        //        prevSwimEvent = swimEvent;
        //    }
        //}
        //static string ReplaceSecondComma(Match match)
        //{
        //    // Replace the second comma with a different character or an empty string
        //    return match.Groups[1].Value + ",\r\n";
        //}
        ////private async void OnAllEventsClicked(object sender, EventArgs e)
        ////{
        ////    var parameter = new DynamicParameters();

        ////    string commandText = "SELECT e.eventNumber AS EventNum, h.heatNumber AS HeatNum, s.laneNumber AS LaneNum, GROUP_CONCAT(s.name, '\r\n') AS SwimmerName, GROUP_CONCAT(s.age, '\r\n') AS Age, s.teamName as TeamName, s.seedTime AS SeedTime, e.eventName AS EventInfo FROM Swimmer s ";
        ////    commandText += "JOIN Heat h ON h.heatId = s.heatId ";
        ////    commandText += "JOIN Event e ON e.eventId = h.eventId ";
        ////    commandText += "GROUP BY e.eventNumber, h.heatNumber, s.laneNumber, s.teamName, s.seedTime ";
        ////    commandText += "ORDER BY eventNumber ASC, heatNumber ASC, laneNumber ASC;";

        ////    var heats = App.InMemoryConnection.Query<HeatInfo>(commandText, param: parameter).ToObservableCollection();

        ////    await Navigation.PushAsync(new AllEvents(heats));

        ////}
        //private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        //{
        //    var parameter = new DynamicParameters();

        //    string commandText = "SELECT e.eventNumber AS EventNum, h.heatNumber AS HeatNum, h.startTime AS StartTime, s.laneNumber AS LaneNum, GROUP_CONCAT(s.name, '\r\n') AS SwimmerName, GROUP_CONCAT(s.age, '\r\n') AS Age, s.teamName as TeamName, s.seedTime AS SeedTime, e.eventName AS EventInfo FROM Swimmer s ";
        //    commandText += "JOIN Heat h ON h.heatId = s.heatId ";
        //    commandText += "JOIN Event e ON e.eventId = h.eventId ";
        //    commandText += "GROUP BY e.eventNumber, h.heatNumber, s.laneNumber, s.teamName, s.seedTime ";
        //    commandText += "ORDER BY eventNumber ASC, heatNumber ASC, laneNumber ASC;";

        //    var heats = App.InMemoryConnection.Query<HeatInfo>(commandText, param: parameter).ToObservableCollection();

        //    if (allEvents is null)
        //    {
        //        allEvents = new AllEvents(heats);
        //    }
        //    await Navigation.PushAsync(allEvents);
        //}
        //private async void OnGridItemTapped(object sender, ItemTappedEventArgs e)
        //{
        //    if ((sender as CollectionView).SelectedItem == null)
        //        return;
        //    var newSelectedItem = e.Item as EventGrid;

        //    var parameter = new DynamicParameters();
        //    parameter.Add("@eventNum", newSelectedItem.EventNum);
        //    parameter.Add("@heatNum", newSelectedItem.HeatNum);

        //    string commandText = "SELECT e.eventNumber AS EventNum, h.heatNumber AS HeatNum, s.laneNumber AS LaneNum, GROUP_CONCAT(s.name, '\r\n') AS SwimmerName, GROUP_CONCAT(s.age, '\r\n') AS Age, s.teamName as TeamName, s.seedTime AS SeedTime FROM Swimmer s ";
        //    commandText += "JOIN Heat h ON h.heatId = s.heatId ";
        //    commandText += "JOIN Event e ON e.eventId = h.eventId ";
        //    commandText += "WHERE eventNum = @eventNum AND heatNum = @heatNum ";
        //    commandText += "GROUP BY e.eventNumber, h.heatNumber, s.laneNumber, s.teamName, s.seedTime ";
        //    commandText += "ORDER BY eventNumber ASC, heatNumber ASC, laneNumber ASC;";

        //    var heats = App.InMemoryConnection.Query<HeatInfo>(commandText, param: parameter).ToList();

        //    await Navigation.PushAsync(new HeatSheetInfo(heats));

        //    EventList.SelectedItem = null;
        //}
    }
}
