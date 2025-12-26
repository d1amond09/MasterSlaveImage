using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using MasterSlaveImage.Maui.Services.Implementations;
using MasterSlaveImage.Maui.Services.Interfaces;
using MasterSlaveImage.Maui.ViewModels;
using Microsoft.Extensions.Logging;

namespace MasterSlaveImage.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<IAlertService, AlertService>();
		builder.Services.AddSingleton<IFilePickerService, FilePickerService>();
		builder.Services.AddSingleton<IZipArchiverService, ZipArchiverService>();
		builder.Services.AddSingleton<IFtpUploadService, FtpUploadService>();
		builder.Services.AddSingleton<IFolderPicker>(FolderPicker.Default);
		builder.Services.AddTransient<MainViewModel>();
		builder.Services.AddTransient<MainPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif
		return builder.Build();
	}
}