using HeatSheetHelper.Core.Interfaces;
using HeatSheetHelper.Core.Helpers;
using HeatSheetHelperBlazor.Services;
using Microsoft.Extensions.Logging;

namespace HeatSheetHelperBlazor;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

        builder.Services.AddMauiBlazorWebView();
		builder.Services.AddSingleton<MeetDataService>();
		builder.Services.AddSingleton<SwimmerListService>();
		builder.Services.AddSingleton<ISwimmerFunctions, SwimmerFunctions>();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
