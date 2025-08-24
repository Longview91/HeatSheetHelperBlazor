using Microsoft.JSInterop;

namespace HeatSheetHelperBlazor.Helpers
{
    public static class StateClass
    {
        public static async Task SaveAsync(IJSRuntime js, string key)
        {
            try
            {
                var scrollY = await js.InvokeAsync<double>("getScrollY");
                await js.InvokeVoidAsync("localStorage.setItem", key, scrollY.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static async Task RestoreAsync(IJSRuntime js, string key)
        {
            try
            {
                var scrollY = await js.InvokeAsync<double>("getStoredScrollY", key);
                await js.InvokeVoidAsync("window.scrollTo", 0, scrollY);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
