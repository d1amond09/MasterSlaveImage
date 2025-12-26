using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterSlaveImage.Maui.Models;
using MasterSlaveImage.Maui.Services.Interfaces;

namespace MasterSlaveImage.Maui.ViewModels;

public partial class MainViewModel : ObservableObject
{
	private readonly IFilePickerService _picker;
	private readonly IZipArchiverService _zipper;
	private readonly IFtpUploadService _ftp;
	private readonly IAlertService _alert;
	private readonly IFolderPicker _folderPicker;

	private CancellationTokenSource _monitorCts = new();

	public MainViewModel(
		IFilePickerService picker,
		IZipArchiverService zipper,
		IFtpUploadService ftp,
		IAlertService alert,
		IFolderPicker folderPicker)
	{
		_picker = picker;
		_zipper = zipper;
		_ftp = ftp;
		_alert = alert;
		_folderPicker = folderPicker;

		StartMonitor();
	}

	public ObservableCollection<string> SystemLogs { get; } = new();
	public ObservableCollection<ImageDisplayItem> Images { get; } = new();
	public ObservableCollection<WorkerStatDto> WorkerStats { get; } = new();

	[ObservableProperty] string ftpHost = "127.0.0.1";
	[ObservableProperty] int ftpPort = 21021;
	[ObservableProperty] string ftpUser = "user";
	[ObservableProperty] string ftpPassword = "password";
	[ObservableProperty] int targetWidth = 800;
	[ObservableProperty] int targetHeight = 600;
	[ObservableProperty] string selectedFormat = "jpg";
	public List<string> Formats { get; } = new() { "jpg", "png", "bmp" };

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(StartProcessingCommand))]
	bool isBusy;

	[ObservableProperty] string statusMessage = "Готов";
	[ObservableProperty] string? currentResultFolder;
	private IEnumerable<FileResult>? _selected;

	private bool CanStartProcessing() => !IsBusy && Images.Any();

	[RelayCommand]
	async Task SelectFiles()
	{
		try
		{
			_selected = await _picker.PickImagesAsync();
			if (_selected == null || !_selected.Any()) return;

			Images.Clear();
			foreach (var f in _selected)
			{
				using var s = await f.OpenReadAsync();
				using var ms = new MemoryStream();
				await s.CopyToAsync(ms);

				Images.Add(new ImageDisplayItem
				{
					FileName = f.FileName,
					OriginalSource = ImageSource.FromStream(() => new MemoryStream(ms.ToArray())),
					FileTypeBadge = "NEW"
				});
			}
			StatusMessage = $"Выбрано: {Images.Count} шт.";

			StartProcessingCommand.NotifyCanExecuteChanged();
		}
		catch (Exception ex) { await _alert.ShowAlertAsync("Ошибка", ex.Message, "OK"); }
	}

	[RelayCommand(CanExecute = nameof(CanStartProcessing))]
	async Task StartProcessing()
	{
		bool confirm = await _alert.ShowConfirmationAsync(
			"Подтверждение",
			$"Вы уверены, что хотите отправить {Images.Count} изображений на обработку?");

		if (!confirm) return; 

		StopMonitor();
		IsBusy = true; 

		SystemLogs.Clear();
		AddLog($"Соединение с {FtpHost}:{FtpPort}...");

		try
		{
			var settings = new FtpSettings(FtpHost, FtpPort, FtpUser, FtpPassword);
			var taskInfo = new TaskInfo(TargetWidth, TargetHeight, SelectedFormat);
			string zipPath = await _zipper.CreateTaskArchiveAsync(_selected!, taskInfo);

			AddLog("Загрузка архива...");
			CurrentResultFolder = await _ftp.UploadFileAsync(zipPath, Path.GetFileName(zipPath), settings, AddLog);

			AddLog($"Ожидание обработки...");
			await DownloadAndDisplayLogic(settings);

			StatusMessage = "Обработка завершена! Нажмите 'Скачать'.";
			AddLog("ГОТОВО.");
		}
		catch (Exception ex)
		{
			AddLog($"ОШИБКА: {ex.Message}");
			StatusMessage = "Ошибка";
			await _alert.ShowAlertAsync("Ошибка отправки", ex.Message, "OK");
		}
		finally
		{
			IsBusy = false;
			StartMonitor();
		}
	}

	[RelayCommand]
	async Task DownloadResults()
	{
		if (string.IsNullOrEmpty(CurrentResultFolder))
		{
			await _alert.ShowAlertAsync("Ошибка", "Нет результатов. Сначала отправьте файлы.", "OK");
			return;
		}

		StopMonitor();
		try
		{
			var folderResult = await _folderPicker.PickAsync(CancellationToken.None);
			if (!folderResult.IsSuccessful)
			{
				StartMonitor();
				return;
			}

			IsBusy = true; 
			string savePath = folderResult.Folder.Path;
			AddLog($"Сохранение в: {savePath}");

			var settings = new FtpSettings(FtpHost, FtpPort, FtpUser, FtpPassword);
			var results = await _ftp.DownloadResultsAsync(CurrentResultFolder, settings, AddLog);

			int savedCount = 0;
			for (int i = 0; i < results.Count; i++)
			{
				byte[] data = results[i];
				string fileName = $"processed_{DateTime.Now.Ticks}_{i}.{SelectedFormat}";
				if (i < Images.Count)
				{
					string origName = Path.GetFileNameWithoutExtension(Images[i].FileName);
					fileName = $"{origName}_processed.{SelectedFormat}";
				}
				string fullPath = Path.Combine(savePath, fileName);
				await File.WriteAllBytesAsync(fullPath, data);
				savedCount++;
			}

			StatusMessage = $"Сохранено файлов: {savedCount}";
			await _alert.ShowAlertAsync("Успех", $"Файлы сохранены в:\n{savePath}", "OK");
		}
		catch (Exception ex)
		{
			AddLog($"Ошибка: {ex.Message}");
			await _alert.ShowAlertAsync("Ошибка", ex.Message, "OK");
		}
		finally
		{
			IsBusy = false;
			StartMonitor();
		}
	}

	private async Task DownloadAndDisplayLogic(FtpSettings settings)
	{
		if (string.IsNullOrEmpty(CurrentResultFolder)) return;

		var results = await _ftp.DownloadResultsAsync(CurrentResultFolder, settings, AddLog);

		int i = 0;
		foreach (var bytes in results)
		{
			if (i < Images.Count)
			{
				var img = Images[i];
				img.ProcessedBytes = bytes;
				MainThread.BeginInvokeOnMainThread(() =>
				{
					img.ProcessedSource = ImageSource.FromStream(() => new MemoryStream(bytes));
					img.FileTypeBadge = SelectedFormat.ToUpper();
				});
				i++;
			}
		}
	}

	private void StartMonitor()
	{
		_monitorCts = new CancellationTokenSource();
		Task.Run(() => MonitorLoop(_monitorCts.Token));
	}

	private void StopMonitor()
	{
		try { _monitorCts.Cancel(); } catch { }
	}

	private async Task MonitorLoop(CancellationToken token)
	{
		var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

		while (!token.IsCancellationRequested)
		{
			if (string.IsNullOrEmpty(FtpHost))
			{
				await Task.Delay(1000, token);
				continue;
			}

			try
			{
				var bytes = await _ftp.DownloadFileBytesAsync("status.json", new FtpSettings(FtpHost, FtpPort, FtpUser, FtpPassword));
				if (bytes != null)
				{
					var json = Encoding.UTF8.GetString(bytes);
					var status = JsonSerializer.Deserialize<ServerStatusDto>(json, jsonOptions);

					if (status?.Workers != null)
					{
						MainThread.BeginInvokeOnMainThread(() =>
						{
							WorkerStats.Clear();
							foreach (var w in status.Workers.OrderBy(x => x.Name)) WorkerStats.Add(w);
						});
					}
				}
			}
			catch { }
			try { await Task.Delay(2000, token); } catch { break; }
		}
	}

	private void AddLog(string msg)
	{
		MainThread.BeginInvokeOnMainThread(() => SystemLogs.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {msg}"));
	}
}