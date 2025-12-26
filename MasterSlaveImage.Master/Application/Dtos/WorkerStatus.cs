namespace MasterSlaveImage.Master.Application.Dtos;

public class WorkerStatus
{
	public string Name { get; set; } = string.Empty;
	public bool IsOnline { get; set; }
	public int ActiveTasks { get; set; }
	public int TotalProcessed { get; set; }
}