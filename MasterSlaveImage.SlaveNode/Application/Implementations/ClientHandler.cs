using System.Net.Sockets;
using System.Text;
using MasterSlaveImage.Shared.Contracts;
using MasterSlaveImage.SlaveNode.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MasterSlaveImage.SlaveNode.Application.Implementations;

public class ClientHandler(ILogger<ClientHandler> logger, IImageProcessor imageProcessor) : IClientHandler
{
	private readonly ILogger<ClientHandler> _logger = logger;
	private readonly IImageProcessor _imageProcessor = imageProcessor;

	public async Task HandleClientAsync(TcpClient client)
	{
		try
		{
			await using var stream = client.GetStream();

			var lengthBuffer = new byte[4];
			int read = await stream.ReadAsync(lengthBuffer.AsMemory(0, 4));
			if (read == 0) return;

			var messageLength = BitConverter.ToInt32(lengthBuffer, 0);
			var messageBuffer = new byte[messageLength];

			int totalRead = 0;
			while (totalRead < messageLength)
			{
				int chunk = await stream.ReadAsync(messageBuffer.AsMemory(totalRead, messageLength - totalRead));
				if (chunk == 0) break;
				totalRead += chunk;
			}

			var requestJson = Encoding.UTF8.GetString(messageBuffer);
			var request = JsonConvert.DeserializeObject<SlaveRequest>(requestJson) 
				?? throw new Exception("Invalid request");

			_logger.LogInformation("Processing file: {FileName}", request.OriginalFileName);

			var (processedBytes, errorMessage) = _imageProcessor.ProcessImage(request.ImageBytes, request.Width, request.Height, request.Format);

			var response = errorMessage == null
				? new SlaveResponse(true, processedBytes, null)
				: new SlaveResponse(false, null, errorMessage);

			var responseJson = JsonConvert.SerializeObject(response);
			var responseBytes = Encoding.UTF8.GetBytes(responseJson);

			await stream.WriteAsync(responseBytes);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error handling client");
		}
		finally
		{
			client.Close();
		}
	}
}