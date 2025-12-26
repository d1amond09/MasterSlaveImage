namespace MasterSlaveImage.Maui.Models;

public class ServerStatusDto
{
	public List<WorkerStatDto> Workers { get; set; } = new();
}