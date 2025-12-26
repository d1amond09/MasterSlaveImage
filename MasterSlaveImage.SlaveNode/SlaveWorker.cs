using MasterSlaveImage.SlaveNode.Application.Interfaces;
using Microsoft.Extensions.Hosting;

namespace MasterSlaveImage.SlaveNode;

public class SlaveWorker(ITcpListenerService listenerService) : BackgroundService
{
	private readonly ITcpListenerService _listenerService = listenerService;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await _listenerService.StartListeningAsync(stoppingToken);
	}
}