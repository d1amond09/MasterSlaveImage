namespace MasterSlaveImage.Master.Domain.Configuration;

public class SlaveNode
{
	public string Name { get; set; } = string.Empty;
	public string Host { get; set; } = string.Empty;
	public int Port { get; set; }
	public int Weight { get; set; } = 1;
	internal int CurrentWeight { get; set; }
}
