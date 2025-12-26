using FubarDev.FtpServer;
using MasterSlaveImage.Master.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MasterSlaveImage.Master.Application.Implementations;

public class CustomFtpServer(IServiceProvider serviceProvider) : ICustomFtpService
{
	private readonly IFtpServerHost _ftpServerHost = serviceProvider.GetRequiredService<IFtpServerHost>();

	public void Start()
	{
		_ftpServerHost.StartAsync(CancellationToken.None).Wait();
		Console.WriteLine(">>> ROBUST FTP SERVER STARTED <<<");
	}

	public void Stop()
	{
		_ftpServerHost.StopAsync(CancellationToken.None).Wait();
	}
}