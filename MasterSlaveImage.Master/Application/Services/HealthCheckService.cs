using System.Net.Sockets;
using MasterSlaveImage.Master.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MasterSlaveImage.Master.Services;

public class HealthCheckService : BackgroundService
{
	private readonly IGlobalState _state;
	private readonly ILogger<HealthCheckService> _logger;

	public HealthCheckService(IGlobalState state, ILogger<HealthCheckService> logger)
	{
		_state = state;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			var slaves = _state.GetAllSlaves();

			var checks = slaves.Select(async slave =>
			{
				bool isAlive = await PingSlaveAsync(slave.Host, slave.Port);

				_state.UpdateSlaveStatus(slave.Name, isAlive);
			});

			await Task.WhenAll(checks);

			await Task.Delay(3000, stoppingToken);
		}
	}

	private async Task<bool> PingSlaveAsync(string host, int port)
	{
		try
		{
			using var client = new TcpClient();
			var connectTask = client.ConnectAsync(host, port);
			var completed = await Task.WhenAny(connectTask, Task.Delay(1000));

			if (completed == connectTask && client.Connected)
			{
				client.Close();
				return true;
			}
			return false;
		}
		catch
		{
			return false;
		}
	}
}