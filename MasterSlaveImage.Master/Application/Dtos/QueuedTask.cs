using MasterSlaveImage.Shared.Contracts;

namespace MasterSlaveImage.Master.Application.Dtos;

public class QueuedTask
{
	public SlaveRequest Request { get; set; }
	public string ResultPath { get; set; } = string.Empty;
}
