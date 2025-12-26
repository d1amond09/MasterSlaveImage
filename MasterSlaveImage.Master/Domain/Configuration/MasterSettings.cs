namespace MasterSlaveImage.Master.Domain.Configuration;

public class MasterSettings
{
	public FtpSettings Ftp { get; set; } = new();
	public PathSettings Paths { get; set; } = new();
	public List<SlaveNode> Slaves { get; set; } = [];
}
