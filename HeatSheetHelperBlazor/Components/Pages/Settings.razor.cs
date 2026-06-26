using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace HeatSheetHelperBlazor.Components.Pages
{
    partial class Settings
    {
        [Inject] private NavigationManager Navigation { get; set; }
        [Inject] private IJSRuntime JS { get; set; }
        private string headerColor = "#8b0000"; // Default to darkred
        private int fontSize = 15; // Default font size
        private string fontSizePx => $"{fontSize}px";

        protected override async Task OnInitializedAsync()
        {
            var color = await JS.InvokeAsync<string>("blazorLocalStorage.get", "tableHeaderColor");
            if (!string.IsNullOrEmpty(color))
                headerColor = color;

            var storedFontSize = await JS.InvokeAsync<string>("blazorLocalStorage.get", "tableFontSize");
            if (int.TryParse(storedFontSize, out var size))
                fontSize = size;

            await JS.InvokeVoidAsync("setTableFontSize", $"{fontSize}px");
        }

        private async Task SaveColor()
        {
            await JS.InvokeVoidAsync("blazorLocalStorage.set", "tableHeaderColor", headerColor);
            await JS.InvokeVoidAsync("setTableHeaderColor", headerColor);
            Navigation.NavigateTo("/");
        }

        private async Task ResetColor()
        {
            headerColor = "#8b0000"; // Reset to default dark red
            await JS.InvokeVoidAsync("blazorLocalStorage.set", "tableHeaderColor", headerColor);
            await JS.InvokeVoidAsync("setTableHeaderColor", headerColor);
            Navigation.NavigateTo("/");
        }

        private async Task SaveFontSize()
        {
            await JS.InvokeVoidAsync("blazorLocalStorage.set", "tableFontSize", fontSize.ToString());
            await JS.InvokeVoidAsync("setTableFontSize", $"{fontSize}px");
            Navigation.NavigateTo("/");
        }
    }
}
