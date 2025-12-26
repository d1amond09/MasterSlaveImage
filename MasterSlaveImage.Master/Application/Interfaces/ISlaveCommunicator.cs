using MasterSlaveImage.Master.Domain.Configuration;
using MasterSlaveImage.Shared.Contracts;

namespace MasterSlaveImage.Master.Application.Interfaces;

public interface ISlaveCommunicator
{
	Task<SlaveResponse> SendTaskAsync(SlaveNode slave, SlaveRequest request);
}
