using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using System.Text.RegularExpressions;
using HeatSheetHelper.Helpers;
using Microsoft.AspNetCore.Components;
using HeatSheetHelperBlazor.Components.Shared;
using HeatSheetHelperBlazor.Models;
using HeatSheetHelperBlazor.Services;

namespace HeatSheetHelperBlazor.Components.Pages
{
    public partial class Home
    {
        [Inject] public MeetDataService MeetDataService { get; set; }
        [Inject] public SwimmerListService SwimmerListService { get; set; }
        private ErrorModal ErrorModal = new();
        private List<SwimmerHeatRow> swimmerHeats = new();

        public string? SelectedSwimmer { get; private set; }

        private async Task OnPDFPickClicked()
        {
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

                var swimMeet = new SwimMeet();
                foreach (var page in document.GetPages())
                {
                    string pdfText = ContentOrderTextExtractor.GetText(page, true);
                    var lines = Regex.Split(pdfText, "\n").ToList();
                    heatSheet.AddRange(Regex.Split(pdfText, "\n").ToList());
                    var eventsToAdd = SwimmerFunctions.ParseHeatSheetToEvents(lines);
                    if (eventsToAdd != null && eventsToAdd.Count > 0)
                    {
                        swimMeet.SwimEvents.AddRange(eventsToAdd);
                    }
                    await InvokeAsync(StateHasChanged); // Update UI after each page
                }

                MeetDataService.SwimMeet = swimMeet;

                //SwimmerFunctions.FillEmptyTimes();

                await PopulateSwimmerNameList();

            }
            catch (Exception ex)
            {
                ErrorModal.Show("Error", "An error was encountered when loading the heat sheet: " + ex.Message);
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

        private async Task SwimmerPicker_SelectedIndexChanged(ChangeEventArgs e)
        {
            var selected = e.Value?.ToString();
            SwimmerListService.SelectedSwimmer = selected;
            if (!string.IsNullOrEmpty(selected))
            {
                await LoadSwimmerHeats(selected);
            }
            else
            {
                swimmerHeats.Clear();
                StateHasChanged();
            }
        }

        private async Task LoadSwimmerHeats(string swimmerName)
        {
            try
            {
                swimmerHeats = await Task.Run(() =>
                {
                    if (MeetDataService.SwimMeet == null || MeetDataService.SwimMeet.SwimEvents == null)
                        return new List<SwimmerHeatRow>();

                    return MeetDataService.SwimMeet.SwimEvents
                        .SelectMany(ev => (ev.Heats ?? new List<HeatInfo>())
                            .SelectMany(heat => (heat.LaneInfos ?? Enumerable.Empty<LaneInfo>())
                                .Select(lane => new { ev, heat, lane })))
                        .Where(x => string.Equals(x.lane.SwimmerName, swimmerName, StringComparison.OrdinalIgnoreCase))
                        .Select(x => new SwimmerHeatRow
                        {
                            EventNumber = x.ev.EventNumber,
                            HeatNumber = x.heat.HeatNumber,
                            LaneNumber = x.lane.LaneNumber,
                            EventName = x.ev.EventDetails,
                            StartTime = x.heat.StartTime,
                            SeedTime = x.lane.SeedTime
                        })
                        .OrderBy(x => x.EventNumber)
                        .ThenBy(x => x.HeatNumber)
                        .ThenBy(x => x.LaneNumber)
                        .ToList();
                });

                StateHasChanged();
            }
            catch (Exception ex)
            {
                ErrorModal.Show("Error", "An error was encountered when loading the swimmer heats: " + ex.Message);
            }
        }

        protected override async Task OnInitializedAsync()
        {
            if (!string.IsNullOrEmpty(SwimmerListService.SelectedSwimmer))
            {
                await LoadSwimmerHeats(SwimmerListService.SelectedSwimmer);
            }
        }
        protected override async Task OnParametersSetAsync()
        {
            if (!string.IsNullOrEmpty(SwimmerListService.SelectedSwimmer))
            {
                await LoadSwimmerHeats(SwimmerListService.SelectedSwimmer);
            }
        }
    }
}
