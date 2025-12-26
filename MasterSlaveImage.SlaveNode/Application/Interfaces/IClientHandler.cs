using System.Net.Sockets;

namespace MasterSlaveImage.SlaveNode.Application.Interfaces;

public interface IClientHandler
{
	Task HandleClientAsync(TcpClient client);
}