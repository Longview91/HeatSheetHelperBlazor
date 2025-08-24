using HeatSheetHelper.Core.Interfaces;
using HeatSheetHelper.Core.Models;
using HeatSheetHelper.Core.Shared;
using HeatSheetHelperBlazor.Helpers;
using HeatSheetHelperBlazor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace HeatSheetHelperBlazor.Components.Pages
{
    public partial class Home
    {
        [Inject] public MeetDataService MeetDataService { get; set; }
        [Inject] public SwimmerListService SwimmerListService { get; set; }
        [Inject] public ISwimmerFunctions SwimmerFunctions { get; set; }
        [Inject] private NavigationManager Navigation { get; set; }
        [Inject] IJSRuntime JS { get; set; }
        [Inject] private IDispatcher Dispatcher { get; set; }
        private ErrorModal ErrorModal = new();
        private List<SwimmerHeatRow> swimmerHeats = new();
        private bool showFilterPanel = true;
        public string? SelectedSwimmer { get; private set; }
        private bool _restoredScroll = false;
        private bool showFavorites = false;
        private bool loading = false;
        private List<string> FavoriteSwimmers = new();
        private System.Timers.Timer? starPressTimer;
        private bool showPopup = false;
        private DateTime touchStartTime = DateTime.MinValue;
        private bool isTouching = false;
        private const int LongPressThresholdMs = 800; // 800ms for long press
        private Task SaveScrollPositionAsync() => StateClass.SaveAsync(JS, "homeScrollY");
        private Task RestoreScrollPositionAsync() => StateClass.RestoreAsync(JS, "homeScrollY");
        private void ToggleFilterPanel()
        {
            showFilterPanel = !showFilterPanel;
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !_restoredScroll)
            {
                _restoredScroll = true;
                await RestoreScrollPositionAsync();
            }
        }
        private async Task OpenSingleHeatAsync(int eventNumber, int heatNumber)
        {
            await SaveScrollPositionAsync();
            Navigation.NavigateTo($"/singleheat/{eventNumber}/{heatNumber}");
        }
        private void OpenSingleHeat(int eventNumber, int heatNumber)
        {
            Navigation.NavigateTo($"/singleheat/{eventNumber}/{heatNumber}");
        }

        private async Task OnPDFPickClicked()
        {
            swimmerHeats.Clear();
            SwimmerListService.SwimmerNameList.Clear();
            SwimmerListService.SelectedSwimmers.Clear();
            SwimmerListService.SwimmerFilter = string.Empty;

            try
            {
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

                loading = true;
                StateHasChanged();

                await Task.Run(async () =>
                {
                    var swimMeet = SwimmerFunctions.ParseHeatSheetToEvents(heatSheet);
                    MeetDataService.SwimMeet = swimMeet;
                    await PopulateSwimmerNameList();
                    await Dispatcher.DispatchAsync(() => StateHasChanged());
                });

                loading = false;

                // Reset scroll position and stored scroll value
                await JS.InvokeVoidAsync("window.scrollTo", 0, 0);
                await JS.InvokeVoidAsync("localStorage.setItem", "homeScrollY", "0");
                await JS.InvokeVoidAsync("localStorage.setItem", "allEventsScrollY", "0");
            }
            catch (Exception ex)
            {
                ErrorModal.Show("Error", "An error was encountered when loading the heat sheet: " + ex.Message);
            }

            if (showFavorites == true)
            {
                await ToggleFavorites();
                ToggleFavorites();
            }

        }

        private async Task PopulateSwimmerNameList()
        {
            if (MeetDataService.SwimMeet == null) return;

            SwimmerListService.SwimmerNameList = await Task.Run(() =>
            {
                return MeetDataService.SwimMeet.SwimEvents
                .SelectMany(ev => ev.Heats)
                .SelectMany(heat => heat.LaneInfos)
                .Select(lane => lane.SwimmerName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .OrderBy(name => name)
                .ToList();
            });
        }
        private async Task ToggleSwimmer(string swimmer)
        {
            if (SwimmerListService.SelectedSwimmers.Contains(swimmer))
                SwimmerListService.SelectedSwimmers.Remove(swimmer);
            else
                SwimmerListService.SelectedSwimmers.Add(swimmer);

            await LoadSwimmerHeatsForSelected();
            StateHasChanged();
        }
        private async Task LoadSwimmerHeatsForSelected()
        {
            var selected = SwimmerListService.SelectedSwimmers;
            if (selected.Count == 0)
            {
                swimmerHeats.Clear();
                return;
            }

            swimmerHeats = await Task.Run(() =>
            {
                if (MeetDataService.SwimMeet == null || MeetDataService.SwimMeet.SwimEvents == null)
                    return new List<SwimmerHeatRow>();

                return MeetDataService.SwimMeet.SwimEvents
                    .SelectMany(ev => (ev.Heats ?? new List<HeatInfo>())
                        .SelectMany(heat => (heat.LaneInfos ?? Enumerable.Empty<LaneInfo>())
                            .Select(lane => new { ev, heat, lane })))
                    .Where(x => selected.Contains(x.lane.SwimmerName))
                    .Select(x => new SwimmerHeatRow
                    {
                        EventNumber = x.ev.EventNumber,
                        HeatNumber = x.heat.HeatNumber,
                        LaneNumber = x.lane.LaneNumber,
                        SwimmerName = x.lane.SwimmerName,
                        EventName = x.ev.EventDetails,
                        StartTime = x.heat.StartTime,
                        SeedTime = x.lane.SeedTime
                    })
                    .OrderBy(x => x.EventNumber)
                    .ThenBy(x => x.HeatNumber)
                    .ThenBy(x => x.LaneNumber)
                    .ToList();
            });
        }
        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadFavoritesFromStorageAsync();
            }
            catch { }
            if (SwimmerListService.SelectedSwimmers != null && SwimmerListService.SelectedSwimmers.Count > 0)
            {
                await LoadSwimmerHeatsForSelected();
            }
        }
        private void StartStarPress()
        {
            if (touchStartTime != DateTime.MinValue) return;
            touchStartTime = DateTime.Now;
            isTouching = true;
        }

        private async Task EndStarPress()
        {
            if (!isTouching) return;
            isTouching = false;

            var touchDuration = (DateTime.Now - touchStartTime).TotalMilliseconds;

            // Debounce with 100ms delay to stabilize touch handling
            await Task.Delay(50);

            if (touchDuration >= LongPressThresholdMs)
            {
                // Long press: show popup
                showPopup = true;
            }
            else
            {
                // Short tap: toggle favorites
                await ToggleFavorites();
            }
            touchStartTime = DateTime.MinValue;
        }
        //private void StartStarPress()
        //{
        //    starPressTimer = new System.Timers.Timer(800); // 800ms for long press
        //    starPressTimer.Elapsed += async (s, e) =>
        //    {
        //        starPressTimer?.Stop();
        //        showPopup = true;
        //        //await SaveFavoritesToFileAsync();
        //    };
        //    starPressTimer.AutoReset = false;
        //    starPressTimer.Start();
        //}

        //private void EndStarPress()
        //{
        //    if (starPressTimer != null && starPressTimer.Enabled)
        //    {
        //        starPressTimer.Stop();
        //        ToggleFavorites();
        //    }
        //}
        private async Task ToggleFavorites()
        {
            showFavorites = !showFavorites;

            if (showFavorites && FavoriteSwimmers.Count > 0)
            {
                SwimmerListService.SelectedSwimmers.Clear();
                foreach (var fav in FavoriteSwimmers)
                {
                    SwimmerListService.SelectedSwimmers.Add(fav);
                }
            }
            else
            {
                SwimmerListService.SelectedSwimmers.Clear();
            }
            await LoadSwimmerHeatsForSelected();
            StateHasChanged();
        }
        private async Task SaveFavoritesToFileAsync()
        {
            // Save the favorites list as a JSON string in localStorage
            await JS.InvokeVoidAsync("localStorage.setItem", "favoriteSwimmers", System.Text.Json.JsonSerializer.Serialize(FavoriteSwimmers));
            showPopup = false;
            StateHasChanged();
        }
        private async Task AppendFavorites()
        {
            // Append selected items, excluding duplicates
            foreach (var item in SwimmerListService.SelectedSwimmers)
            {
                if (!FavoriteSwimmers.Contains(item))
                {
                    FavoriteSwimmers.Add(item);
                }
            }
            // Save updated favorites
            await SaveFavoritesToFileAsync();
        }
        private async Task OverwriteFavorites()
        {
            // Copy currently checked swimmers to favorites
            FavoriteSwimmers = SwimmerListService.SelectedSwimmers.ToList();
            await SaveFavoritesToFileAsync();
        }
        private void ClosePopup()
        {
            showPopup = false;
            StateHasChanged();
        }
        private async Task LoadFavoritesFromStorageAsync()
        {
            var json = await JS.InvokeAsync<string>("localStorage.getItem", "favoriteSwimmers");
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    var loaded = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);
                    if (loaded != null)
                        FavoriteSwimmers = loaded;
                }
                catch
                {
                    FavoriteSwimmers = new List<string>();
                }
            }
        }
    }
}
