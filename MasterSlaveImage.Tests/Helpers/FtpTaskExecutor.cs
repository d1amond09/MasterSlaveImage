using FluentFTP;
using Xunit.Abstractions;

namespace MasterSlaveImage.Tests.Helpers;

public static class FtpTaskExecutor
{
	public static async Task<bool> ExecuteAsync(string zipPath, int expectedCount, string host, int port, string user, string pass, ITestOutputHelper log)
	{
		using var client = new FtpClient(host, user, pass, port);
		client.Config.EncryptionMode = FtpEncryptionMode.None;
		client.Config.DataConnectionType = FtpDataConnectionType.PASV;
		client.Config.InternetProtocolVersions = FtpIpVersion.IPv4;
		client.Config.CheckCapabilities = false;
		client.Config.ConnectTimeout = 5000;

		try
		{
			client.Connect();

			string remoteName = Path.GetFileName(zipPath);
			client.UploadFile(zipPath, $"/uploads/{remoteName}", FtpRemoteExists.Overwrite);

			string resultFolder = $"/results/result_{Path.GetFileNameWithoutExtension(remoteName)}";
			int pollIntervalMs = 100; 
			int maxRetries = 600;

			for (int i = 0; i < maxRetries; i++)
			{
				if (client.DirectoryExists(resultFolder))
				{
					var files = client.GetListing(resultFolder);
					int current = files.Count(f => f.Type == FtpObjectType.File);
					if (current >= expectedCount) return true;
				}
				await Task.Delay(pollIntervalMs);
			}
			return false;
		}
		catch (Exception ex)
		{
			log.WriteLine($"FTP FAIL: {ex.Message}");
			return false;
		}
	}
}
