using MasterSlaveImage.Maui.Models;

namespace MasterSlaveImage.Maui.Services.Interfaces;

public interface IFtpUploadService
{
	Task<string> UploadFileAsync(string localPath, string remoteFileName, FtpSettings settings, Action<string>? onLog = null);
	Task<List<DownloadedFile>> DownloadResultsAsync(string resultFolderName, int expectedCount, FtpSettings settings, Action<string>? onLog = null);
	Task<byte[]?> DownloadFileBytesAsync(string remotePath, FtpSettings settings);
}
