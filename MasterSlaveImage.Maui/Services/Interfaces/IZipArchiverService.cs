using MasterSlaveImage.Maui.Models;

namespace MasterSlaveImage.Maui.Services.Interfaces;

public interface IZipArchiverService
{
	Task<string> CreateTaskArchiveAsync(IEnumerable<FileResult> files, TaskInfo taskInfo);
}