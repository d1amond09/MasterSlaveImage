using System.IO.Compression;
using System.Text.Json;
using MasterSlaveImage.Maui.Models;
using MasterSlaveImage.Maui.Services.Interfaces;

namespace MasterSlaveImage.Maui.Services.Implementations;

public class ZipArchiverService : IZipArchiverService
{
	public async Task<string> CreateTaskArchiveAsync(IEnumerable<FileResult> files, TaskInfo taskInfo)
	{
		string temp = Path.Combine(FileSystem.CacheDirectory, $"task_{Guid.NewGuid()}.zip");
		using var archive = ZipFile.Open(temp, ZipArchiveMode.Create);

		var entry = archive.CreateEntry("task.json");
		using (var sw = new StreamWriter(entry.Open()))
		{
			await sw.WriteAsync(JsonSerializer.Serialize(taskInfo));
		}

		foreach (var file in files)
		{
			archive.CreateEntryFromFile(file.FullPath, file.FileName);
		}
		return temp;
	}
}
