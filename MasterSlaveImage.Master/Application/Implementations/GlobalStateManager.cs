using System.Collections.Concurrent;
using MasterSlaveImage.Master.Application.Dtos;
using MasterSlaveImage.Master.Application.Interfaces;
using MasterSlaveImage.Master.Domain.Configuration;
using Microsoft.Extensions.Options;

namespace MasterSlaveImage.Master.Application.Implementations;

public class GlobalStateManager : IGlobalState
{
	private readonly ConcurrentQueue<QueuedTask> _taskQueue = new();
	private readonly List<SlaveNode> _slaves;

	private readonly ConcurrentDictionary<string, WorkerStatus> _workerStats = new();

	public GlobalStateManager(IOptions<MasterSettings> settings)
	{
		_slaves = settings.Value.Slaves;

		foreach (var slave in _slaves)
		{
			_workerStats[slave.Name] = new WorkerStatus
			{
				Name = slave.Name,
				IsOnline = false, 
				ActiveTasks = 0,
				TotalProcessed = 0
			};
		}
	}

	public void EnqueueTask(QueuedTask task) => _taskQueue.Enqueue(task);
	public bool TryDequeueTask(out QueuedTask task) => _taskQueue.TryDequeue(out task);
	public List<SlaveNode> GetAllSlaves() => _slaves;

	public void UpdateSlaveStatus(string name, bool isOnline, int activeDelta = 0, bool incrementTotal = false)
	{
		if (_workerStats.TryGetValue(name, out var stats))
		{
			stats.IsOnline = isOnline;

			int newActive = stats.ActiveTasks + activeDelta;
			if (newActive < 0) newActive = 0;
			stats.ActiveTasks = newActive;

			if (incrementTotal) stats.TotalProcessed++;
		}
	}

	public List<WorkerStatus> GetStats() => _workerStats.Values.OrderBy(x => x.Name).ToList();
}