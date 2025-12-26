namespace MasterSlaveImage.Master.Domain.Configuration;

public class FtpSettings
{
	public int Port { get; set; }
	public string User { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
}
