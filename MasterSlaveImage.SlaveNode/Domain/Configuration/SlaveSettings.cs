namespace MasterSlaveImage.SlaveNode.Domain.Configuration;

public class SlaveSettings
{
	public int ListenPort { get; set; }
	public string Name { get; set; } = "Unnamed Slave";
}
