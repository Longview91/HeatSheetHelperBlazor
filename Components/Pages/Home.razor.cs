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
using HeatSheetHelperBlazor.Components.Shared;
using HeatSheetHelperBlazor.Models;

namespace HeatSheetHelperBlazor.Components.Pages
{
    public partial class Home
    {
        private ErrorModal ErrorModal = new();
        private List<string> swimmerNameList = new();
        private List<SwimmerHeatRow> swimmerHeats = new();
        private List<Swimmer> allSwimmers = new();
        private SwimMeet swimMeet;

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

                swimMeet = SwimmerFunctions.ParseHeatSheetToEvents(heatSheet);
                //allSwimmers = SwimmerFunctions.PutHeatSheetInClasses(heatSheet);

                SwimmerFunctions.FillEmptyTimes();

                await PopulateSwimmerNameList();
                //await CreateInitialList();

            }
            catch (Exception ex)
            {
                ErrorModal.Show("Error", "An error was encountered when loading the heat sheet: " + ex.Message);
            }
        }

        private async Task PopulateSwimmerNameList()
        {
            if (swimMeet == null) return;

            swimmerNameList = await Task.Run(() =>
            {
                return swimMeet.SwimEvents
                .SelectMany(ev => ev.Heats)
                .SelectMany(heat => heat.LaneInfos)
                .Select(lane => lane.SwimmerName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .OrderBy(name => name)
                .ToList();
            });
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
                    if (swimMeet == null || swimMeet.SwimEvents == null)
                        return new List<SwimmerHeatRow>();

                    return swimMeet.SwimEvents
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
                            StartTime = x.heat.StartTime
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
    }
}
