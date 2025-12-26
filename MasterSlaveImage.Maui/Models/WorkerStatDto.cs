namespace MasterSlaveImage.Maui.Models;

public class WorkerStatDto
{
	public string Name { get; set; }
	public bool IsOnline { get; set; }
	public int ActiveTasks { get; set; }
	public int TotalProcessed { get; set; }

	public string StatusText => IsOnline ? (ActiveTasks > 0 ? $"В РАБОТЕ ({ActiveTasks})" : "ДОСТУПЕН") : "НЕДОСТУПЕН";
	public Color StatusColor => IsOnline ? (ActiveTasks > 0 ? Colors.Orange : Colors.LimeGreen) : Colors.Red;
}
