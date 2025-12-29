using FluentFTP;
using MasterSlaveImage.Maui.Models;
using MasterSlaveImage.Maui.Services.Interfaces;

namespace MasterSlaveImage.Maui.Services.Implementations;

public class FtpUploadService : IFtpUploadService
{
	private AsyncFtpClient Create(FtpSettings s)
	{
		var client = new AsyncFtpClient(s.Host, s.Username, s.Password, s.Port);
		client.Config.ConnectTimeout = 5000;
		client.Config.DataConnectionType = FtpDataConnectionType.AutoPassive;
		return client;
	}

	public async Task<string> UploadFileAsync(string localPath, string remoteFileName, FtpSettings settings, Action<string>? onLog = null)
	{
		using var client = Create(settings);
		try
		{
			onLog?.Invoke($"Connecting to {settings.Host}:{settings.Port}...");
			await client.Connect();

			string remotePath = $"/uploads/{remoteFileName}";

			if (!await client.DirectoryExists("/uploads")) await client.CreateDirectory("/uploads");

			onLog?.Invoke("Uploading...");

			var status = await client.UploadFile(localPath, remotePath, FtpRemoteExists.Overwrite);

			if (status == FtpStatus.Failed) throw new Exception("Upload failed.");

			await client.Disconnect();
			onLog?.Invoke("Upload Success!");

			return $"result_{Path.GetFileNameWithoutExtension(remoteFileName)}";
		}
		catch (Exception ex)
		{
			onLog?.Invoke($"Upload Error: {ex.Message}");
			throw;
		}
	}

	public async Task<List<DownloadedFile>> DownloadResultsAsync(string resultFolderName, int expectedCount, FtpSettings settings, Action<string>? onLog = null)
	{
		var resultList = new List<DownloadedFile>();
		string targetDir = $"/results/{resultFolderName}";

		onLog?.Invoke($"Жду {expectedCount} файлов в: {targetDir}");

		for (int i = 0; i < 30; i++)
		{
			using var client = Create(settings);
			try
			{
				await client.Connect();

				if (await client.DirectoryExists(targetDir))
				{
					var items = await client.GetListing(targetDir);

					var filesOnly = items.Where(f => f.Type == FtpObjectType.File).ToList();

					if (filesOnly.Count > 0)
					{
						onLog?.Invoke($"Найдено файлов: {filesOnly.Count} из {expectedCount}");
					}

					if (filesOnly.Count >= expectedCount)
					{
						await Task.Delay(1000);

						onLog?.Invoke("Скачивание...");
						foreach (var f in filesOnly)
						{
							var bytes = await client.DownloadBytes(f.FullName, CancellationToken.None);
							if (bytes != null)
							{
								resultList.Add(new DownloadedFile
								{
									FileName = f.Name,
									Data = bytes
								});
							}
						}

						await client.Disconnect();
						return resultList; 
					}
				}
				await client.Disconnect();
			}
			catch { }

			if (i % 3 == 0) onLog?.Invoke($"Processing... ({i})");
			await Task.Delay(2000);
		}
		throw new TimeoutException("Timeout waiting for results.");
	}

	public async Task<byte[]?> DownloadFileBytesAsync(string remotePath, FtpSettings settings)
	{
		using var client = Create(settings);
		try { 
			await client.Connect();
			return await client.DownloadBytes(remotePath, CancellationToken.None);
		}
		catch 
		{ 
			return null; 
		}
	}
}