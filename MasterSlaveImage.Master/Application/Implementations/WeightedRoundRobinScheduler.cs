using MasterSlaveImage.Master.Application.Interfaces;
using MasterSlaveImage.Master.Domain.Configuration;
using Microsoft.Extensions.Options;

namespace MasterSlaveImage.Master.Application.Implementations;

public class WeightedRoundRobinScheduler(IOptions<MasterSettings> settings, IGlobalState state) : ISlaveScheduler
{
	private readonly List<SlaveNode> _slaves = settings.Value.Slaves;
	private readonly IGlobalState _state = state;
	private readonly Lock _lock = new();

	public SlaveNode GetNextSlave()
	{
		lock (_lock)
		{
			var stats = _state.GetStats(); 

			var candidates = new List<(SlaveNode Node, double LoadScore)>();

			foreach (var slave in _slaves)
			{
				var stat = stats.FirstOrDefault(s => s.Name == slave.Name);

				if (stat == null || !stat.IsOnline) continue;

				double weight = slave.Weight > 0 ? slave.Weight : 1;
				double score = stat.ActiveTasks / weight;

				candidates.Add((slave, score));
			}

			if (candidates.Count == 0) return null;

			var bestCandidate = candidates.OrderBy(x => x.LoadScore).First();

			return bestCandidate.Node;
		}
	}
}