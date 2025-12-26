using MasterSlaveImage.Master.Application.Interfaces;
using MasterSlaveImage.Master.Domain.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace MasterSlaveImage.Master.Application.Implementations;

public class StatusWriterService(IGlobalState state, IOptions<MasterSettings> opt) : BackgroundService
{
	private readonly IGlobalState _state = state;
	private readonly string _path = opt.Value.Paths.FtpRoot;

	protected override async Task ExecuteAsync(CancellationToken ct)
	{
		while (!ct.IsCancellationRequested)
		{
			try
			{
				var report = new { Workers = _state.GetStats() };
				await File.WriteAllTextAsync(Path.Combine(_path, "status.json"), JsonConvert.SerializeObject(report), ct);
			}
			catch { }
			await Task.Delay(2000, ct);
		}
	}
}