using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSheetHelperBlazor.Components.Pages
{
    partial class Settings
    {
        [Inject] private NavigationManager Navigation { get; set; }
        [Inject] private IJSRuntime JS { get; set; }
        private string headerColor = "#8b0000"; // Default to darkred

        protected override async Task OnInitializedAsync()
        {
            var color = await JS.InvokeAsync<string>("blazorLocalStorage.get", "tableHeaderColor");
            if (!string.IsNullOrEmpty(color))
                headerColor = color;
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
    }
}
