using MasterSlaveImage.Master.Application.Interfaces;
using MasterSlaveImage.Master.Domain.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MasterSlaveImage.Master.Application.Implementations;

public class FileWatcherService(IOptions<MasterSettings> opt, ITaskProcessor proc) : IFileWatcherService
{
	private readonly string _path = Path.Combine(opt.Value.Paths.FtpRoot, "uploads");
	private readonly ITaskProcessor _proc = proc;
	private bool _running;

	public void StartWatching()
	{
		_running = true;
		Task.Run(async () =>
		{
			while (_running)
			{
				try
				{
					if (Directory.Exists(_path))
					{
						foreach (var f in Directory.GetFiles(_path, "*.zip"))
						{
							await _proc.ProcessTaskFileAsync(f);
						}
					}
				}
				catch { }
				await Task.Delay(1000);
			}
		});
	}
}