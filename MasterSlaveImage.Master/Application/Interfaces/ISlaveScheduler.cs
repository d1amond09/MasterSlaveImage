using MasterSlaveImage.Master.Domain.Configuration;

namespace MasterSlaveImage.Master.Application.Interfaces;

public interface ISlaveScheduler
{
	SlaveNode GetNextSlave();
}
