using System.Net.Sockets;
using System.Text;
using MasterSlaveImage.Master.Application.Interfaces;
using MasterSlaveImage.Master.Domain.Configuration;
using MasterSlaveImage.Shared.Contracts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MasterSlaveImage.Master.Application.Implementations;

public class SlaveCommunicator(ILogger<SlaveCommunicator> logger) : ISlaveCommunicator
{
	private readonly ILogger<SlaveCommunicator> _logger = logger;

	public async Task<SlaveResponse> SendTaskAsync(SlaveNode slave, SlaveRequest request)
	{
		try
		{
			using var client = new TcpClient();
			await client.ConnectAsync(slave.Host, slave.Port);
			await using var stream = client.GetStream();

			var requestJson = JsonConvert.SerializeObject(request);
			var requestBytes = Encoding.UTF8.GetBytes(requestJson);
			await stream.WriteAsync(BitConverter.GetBytes(requestBytes.Length));
			await stream.WriteAsync(requestBytes);

			using var ms = new MemoryStream();
			await stream.CopyToAsync(ms);
			var responseJson = Encoding.UTF8.GetString(ms.ToArray());
			return JsonConvert.DeserializeObject<SlaveResponse>(responseJson) ?? new SlaveResponse(false, null, "Null response");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Communication error with {Slave}", slave.Name);
			return new SlaveResponse(false, null, ex.Message);
		}
	}
}