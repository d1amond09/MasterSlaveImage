namespace MasterSlaveImage.SlaveNode.Application.Interfaces;

public interface ITcpListenerService
{
	Task StartListeningAsync(CancellationToken cancellationToken);
}