using HeatSheetHelperBlazor.Components.Shared;
using HeatSheetHelperBlazor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace HeatSheetHelperBlazor.Components.Pages
{
    partial class AllEvents
    {
        private int? selectedEventNumber;
        private int? selectedHeatNumber;
        private List<int> eventNumbers = new();
        private List<int> heatNumbers = new();
        private List<string> teamNames = new();
        private string selectedTeamName = "";

        protected override void OnInitialized()
        {
            base.OnInitialized();
            LoadTeamNames();
        }

        private void LoadTeamNames()
        {
            if (MeetDataService.SwimMeet?.SwimEvents == null)
                return;

            teamNames = MeetDataService.SwimMeet.SwimEvents
                .SelectMany(ev => ev.Heats)
                .SelectMany(h => h.LaneInfos)
                .Select(l => l.TeamName)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .OrderBy(t => t)
                .ToList();
        }

        private void OnTeamNameChanged(ChangeEventArgs e)
        {
            selectedTeamName = e.Value?.ToString() ?? "";
            StateHasChanged();
        }

        protected override void OnParametersSet()
        {
            if (MeetDataService.SwimMeet?.SwimEvents != null)
            {
                eventNumbers = MeetDataService.SwimMeet.SwimEvents
                    .Select(e => e.EventNumber)
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();
            }
        }

        private void OnEventNumberChanged(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out var evtNum))
            {
                selectedEventNumber = evtNum;
                var evt = MeetDataService.SwimMeet.SwimEvents.FirstOrDefault(ev => ev.EventNumber == evtNum);
                heatNumbers = evt?.Heats.Select(h => h.HeatNumber).Distinct().OrderBy(n => n).ToList() ?? new List<int>();
                selectedHeatNumber = heatNumbers.FirstOrDefault();
                ScrollToTable(evtNum, selectedHeatNumber ?? 1);
            }
        }

        private void OnHeatNumberChanged(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out var heatNum))
            {
                selectedHeatNumber = heatNum;
                if (selectedEventNumber.HasValue)
                    ScrollToTable(selectedEventNumber.Value, heatNum);
            }
        }

        private async void ScrollToTable(int eventNumber, int heatNumber)
        {
            await JS.InvokeVoidAsync("scrollToEventHeat", eventNumber, heatNumber);
        }

        [Inject] private IJSRuntime JS { get; set; }
    }
}
