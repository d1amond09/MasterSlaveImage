using MasterSlaveImage.Master.Application.Dtos;
using MasterSlaveImage.Master.Domain.Configuration;

namespace MasterSlaveImage.Master.Application.Interfaces;

public interface IGlobalState
{
	void EnqueueTask(QueuedTask task);
	bool TryDequeueTask(out QueuedTask task);

	List<SlaveNode> GetAllSlaves();
	void UpdateSlaveStatus(string name, bool isOnline, int activeDelta = 0, bool incrementTotal = false);
	List<WorkerStatus> GetStats();
}
