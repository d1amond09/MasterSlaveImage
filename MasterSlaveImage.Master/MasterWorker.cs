using MasterSlaveImage.Master.Application.Interfaces;
using Microsoft.Extensions.Hosting;

namespace MasterSlaveImage.Master;

public class MasterWorker(ICustomFtpService ftp, IFileWatcherService watcher) : BackgroundService
{
    private readonly ICustomFtpService _ftp = ftp;
    private readonly IFileWatcherService _watcher = watcher;

	protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _ftp.Start();
        _watcher.StartWatching();
        await Task.Delay(-1, ct);
    }

    public override Task StopAsync(CancellationToken ct)
    {
        _ftp.Stop();
        return base.StopAsync(ct);
    }
}