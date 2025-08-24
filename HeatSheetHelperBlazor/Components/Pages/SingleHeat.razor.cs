using HeatSheetHelper.Core.Shared;
using HeatSheetHelperBlazor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace HeatSheetHelperBlazor.Components.Pages
{
    partial class SingleHeat
    {
        [Parameter] public int EventNumber { get; set; }
        [Parameter] public int HeatNumber { get; set; }

        [Inject] public MeetDataService MeetDataService { get; set; }
        [Inject] NavigationManager Navigation { get; set; }
        [Inject] IJSRuntime JS { get; set; }

        private HeatInfo? heatInfo;
        private int? selectedEventNumber;
        private int? selectedHeatNumber;
        private List<int> eventNumbers = new();
        private List<int> heatNumbers = new();
        private int? heatsBetween;
        private string? eventDetails;

        async Task OnBackClicked()
        {
            {
                await JS.InvokeVoidAsync("history.back");
            }
        }
        protected override void OnParametersSet()
        {
            var swimMeet = MeetDataService.SwimMeet;
            if (swimMeet?.SwimEvents != null)
            {
                eventNumbers = swimMeet.SwimEvents
                    .Where(e => e.EventNumber < EventNumber || e.EventNumber == EventNumber)
                    .Select(e => e.EventNumber)
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();

                eventDetails = swimMeet.SwimEvents.FirstOrDefault(e => e.EventNumber == EventNumber)?.EventDetails;

                if (selectedEventNumber.HasValue)
                {
                    var evt = swimMeet.SwimEvents.FirstOrDefault(e => e.EventNumber == selectedEventNumber.Value);
                    if (evt != null)
                    {
                        if (selectedEventNumber == EventNumber)
                        {
                            heatNumbers = evt.Heats
                                .Where(h => h.HeatNumber < HeatNumber)
                                .Select(h => h.HeatNumber)
                                .Distinct()
                                .OrderBy(n => n)
                                .ToList();
                        }
                        else
                        {
                            heatNumbers = evt.Heats
                                .Select(h => h.HeatNumber)
                                .Distinct()
                                .OrderBy(n => n)
                                .ToList();
                        }
                    }
                    else
                    {
                        heatNumbers = new List<int>();
                    }
                }
                else
                {
                    heatNumbers = new List<int>();
                }

                var currentEvt = swimMeet.SwimEvents.FirstOrDefault(e => e.EventNumber == EventNumber);
                heatInfo = currentEvt?.Heats.FirstOrDefault(h => h.HeatNumber == HeatNumber);
            }
        }

        private void OnEventNumberChanged(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out var evtNum))
            {
                selectedEventNumber = evtNum;
                var swimMeet = MeetDataService.SwimMeet;
                var evt = swimMeet?.SwimEvents.FirstOrDefault(ev => ev.EventNumber == evtNum);
                if (evt != null)
                {
                    if (selectedEventNumber == EventNumber)
                    {
                        heatNumbers = evt.Heats
                            .Where(h => h.HeatNumber < HeatNumber)
                            .Select(h => h.HeatNumber)
                            .Distinct()
                            .OrderBy(n => n)
                            .ToList();
                    }
                    else
                    {
                        heatNumbers = evt.Heats
                            .Select(h => h.HeatNumber)
                            .Distinct()
                            .OrderBy(n => n)
                            .ToList();
                    }
                }
                else
                {
                    heatNumbers = new List<int>();
                }
                selectedHeatNumber = null;
                heatsBetween = null;
                StateHasChanged();
            }
        }

        private void OnHeatNumberChanged(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out var heatNum))
            {
                selectedHeatNumber = heatNum;
                CalculateHeatsBetween();
                StateHasChanged();
            }
        }

        private void CalculateHeatsBetween()
        {
            var swimMeet = MeetDataService.SwimMeet;
            if (swimMeet?.SwimEvents == null || !selectedEventNumber.HasValue || !selectedHeatNumber.HasValue)
            {
                heatsBetween = null;
                return;
            }

            // Flatten all heats with their event/heat numbers, ordered
            var allHeats = swimMeet.SwimEvents
                .OrderBy(e => e.EventNumber)
                .SelectMany(e => e.Heats.OrderBy(h => h.HeatNumber)
                    .Select(h => new { e.EventNumber, h.HeatNumber }))
                .ToList();

            // Find indices
            int fromIndex = allHeats.FindIndex(x => x.EventNumber == selectedEventNumber && x.HeatNumber == selectedHeatNumber);
            int toIndex = allHeats.FindIndex(x => x.EventNumber == EventNumber && x.HeatNumber == HeatNumber);

            if (fromIndex != -1 && toIndex != -1 && fromIndex < toIndex)
                heatsBetween = toIndex - fromIndex;
            else
                heatsBetween = null;
        }
    }
}
