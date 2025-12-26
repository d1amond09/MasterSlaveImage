using System.IO.Compression;
using MasterSlaveImage.Master.Application.Dtos;
using MasterSlaveImage.Master.Application.Interfaces;
using MasterSlaveImage.Master.Domain.Configuration;
using MasterSlaveImage.Shared.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace MasterSlaveImage.Master.Application.Implementations;

public class TaskProcessor : ITaskProcessor
{
	private readonly IGlobalState _state;
	private readonly ILogger<TaskProcessor> _logger;
	private readonly string _resultsRoot;

	public TaskProcessor(IGlobalState state, ILogger<TaskProcessor> logger, IOptions<MasterSettings> settings)
	{
		_state = state;
		_logger = logger;
		_resultsRoot = Path.Combine(settings.Value.Paths.FtpRoot, "results");
		Directory.CreateDirectory(_resultsRoot);
	}

	public async Task ProcessTaskFileAsync(string zipFilePath)
	{
		if (!await WaitForFile(zipFilePath)) return;

		try
		{
			string taskName = Path.GetFileNameWithoutExtension(zipFilePath);
			string resultDir = Path.Combine(_resultsRoot, $"result_{taskName}");
			Directory.CreateDirectory(resultDir);

			using var archive = ZipFile.OpenRead(zipFilePath);
			var taskEntry = archive.GetEntry("task.json");
			if (taskEntry == null) return;

			using var reader = new StreamReader(taskEntry.Open());
			var info = JsonConvert.DeserializeObject<ClientTaskInfo>(await reader.ReadToEndAsync());

			if (info == null) return;

			foreach (var entry in archive.Entries)
			{
				if (entry.Name == "task.json") continue;
				using var ms = new MemoryStream();
				using var es = entry.Open();
				await es.CopyToAsync(ms);

				_state.EnqueueTask(new QueuedTask
				{
					ResultPath = resultDir,
					Request = new SlaveRequest(ms.ToArray(), info.Width, info.Height, info.Format, entry.Name)
				});
			}
			_logger.LogInformation("Enqueued tasks from {File}", zipFilePath);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Processing error");
		}
		finally
		{
			try { File.Delete(zipFilePath); } catch { }
		}
	}

	private async Task<bool> WaitForFile(string path)
	{
		for (int i = 0; i < 10; i++)
		{
			try
			{
				using var s = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
				return true;
			}
			catch { await Task.Delay(500); }
		}
		return false;
	}
}