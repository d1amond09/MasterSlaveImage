using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using MasterSlaveImage.SlaveNode.Application.Interfaces;
using MasterSlaveImage.SlaveNode.Domain.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MasterSlaveImage.SlaveNode.Application.Implementations;

public class TcpListenerService(ILogger<TcpListenerService> logger, IOptions<SlaveSettings> settings, IServiceProvider serviceProvider) : ITcpListenerService
{
	private readonly ILogger<TcpListenerService> _logger = logger;
	private readonly IServiceProvider _serviceProvider = serviceProvider;
	private readonly SlaveSettings _settings = settings.Value;

	public async Task StartListeningAsync(CancellationToken cancellationToken)
	{
		var listener = new TcpListener(IPAddress.Any, _settings.ListenPort);
		try
		{
			listener.Start();
			_logger.LogInformation("{SlaveName} запущен и слушает порт {Port}", _settings.Name, _settings.ListenPort);

			while (!cancellationToken.IsCancellationRequested)
			{
				var client = await listener.AcceptTcpClientAsync(cancellationToken);

				_ = Task.Run(async () =>
				{
					using var scope = _serviceProvider.CreateScope();
					var clientHandler = scope.ServiceProvider.GetRequiredService<IClientHandler>();
					await clientHandler.HandleClientAsync(client);
				}, cancellationToken);
			}
		}
		catch (OperationCanceledException)
		{
			_logger.LogInformation("{SlaveName} останавливает прослушивание порта.", _settings.Name);
		}
		catch (Exception ex)
		{
			_logger.LogCritical(ex, "Критическая ошибка в TCP Listener.");
		}
		finally
		{
			listener.Stop();
		}
	}
}
