using MasterSlaveImage.Master.Domain.Configuration;

namespace MasterSlaveImage.Master.Application.Interfaces;

public interface ITaskProcessor
{
	Task ProcessTaskFileAsync(string zipFilePath);
}
