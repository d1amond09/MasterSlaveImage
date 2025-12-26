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

	public async Task<List<byte[]>> DownloadResultsAsync(string resultFolderName, FtpSettings settings, Action<string>? onLog = null)
	{
		var list = new List<byte[]>();
		string targetDir = $"/results/{resultFolderName}";

		onLog?.Invoke($"Waiting for results...");

		for (int i = 0; i < 30; i++)
		{
			using var client = Create(settings);
			try
			{
				await client.Connect();
				if (await client.DirectoryExists(targetDir))
				{
					var files = await client.GetListing(targetDir);
					if (files.Any(f => f.Type == FtpObjectType.File))
					{
						onLog?.Invoke($"Downloading {files.Length} files...");
						foreach (var f in files)
						{
							if (f.Type == FtpObjectType.File)
							{
								var bytes = await client.DownloadBytes(f.FullName, CancellationToken.None);
								if (bytes != null) list.Add(bytes);
							}
						}
						await client.Disconnect();
						return list;
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
		try
		{
			await client.Connect();
			return await client.DownloadBytes(remotePath, CancellationToken.None);
		}
		catch { return null; }
	}
}