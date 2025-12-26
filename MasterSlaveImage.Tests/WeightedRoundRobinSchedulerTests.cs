using FluentAssertions;
using MasterSlaveImage.Master.Application.Dtos;
using MasterSlaveImage.Master.Application.Implementations;
using MasterSlaveImage.Master.Application.Interfaces;
using MasterSlaveImage.Master.Domain.Configuration;
using Microsoft.Extensions.Options;
using Moq;

namespace MasterSlaveImage.Tests;

public class WeightedRoundRobinSchedulerTests
{
	private readonly Mock<IOptions<MasterSettings>> _optionsMock;
	private readonly Mock<IGlobalState> _stateMock;
	private readonly List<Master.Domain.Configuration.SlaveNode> _slaves;
	private WeightedRoundRobinScheduler _scheduler;
	public WeightedRoundRobinSchedulerTests()
	{
		_slaves =
		[
			new() { Name = "Weak", Weight = 1 },
			new() { Name = "Strong", Weight = 10 }
		];

		_optionsMock = new Mock<IOptions<MasterSettings>>();
		_optionsMock.Setup(o => o.Value).Returns(new MasterSettings { Slaves = _slaves });

		_stateMock = new Mock<IGlobalState>();
	}

	[Fact]
	public void GetNextSlave_ShouldReturnNull_WhenNoSlavesOnline()
	{
		var stats = new List<WorkerStatus>
	{
		new() { Name = "Weak", IsOnline = false },
		new() { Name = "Strong", IsOnline = false }
	};
		_stateMock.Setup(s => s.GetStats()).Returns(stats);

		_scheduler = new WeightedRoundRobinScheduler(_optionsMock.Object, _stateMock.Object);

		var result = _scheduler.GetNextSlave();
		result.Should().BeNull();
	}

	[Fact]
	public void GetNextSlave_ShouldReturnOnlyOnlineSlave()
	{
		var stats = new List<WorkerStatus>
	{
		new() { Name = "Weak", IsOnline = true },
		new() { Name = "Strong", IsOnline = false }
	};
		_stateMock.Setup(s => s.GetStats()).Returns(stats);

		_scheduler = new WeightedRoundRobinScheduler(_optionsMock.Object, _stateMock.Object);

		var result = _scheduler.GetNextSlave();
		result.Name.Should().Be("Weak");
	}

	[Fact]
	public void GetNextSlave_ShouldPrioritizeLowerLoadScore()
	{
		var stats = new List<WorkerStatus>
		{
			new() { Name = "Weak", IsOnline = true, ActiveTasks = 2 },
			new() { Name = "Strong", IsOnline = true, ActiveTasks = 5 }
		};
		_stateMock.Setup(s => s.GetStats()).Returns(stats);

		_scheduler = new WeightedRoundRobinScheduler(_optionsMock.Object, _stateMock.Object);

		var result = _scheduler.GetNextSlave();

		result.Name.Should().Be("Strong");
	}

	[Fact]
	public void GetNextSlave_ShouldPickEmptyNodeFirst()
	{
		var stats = new List<WorkerStatus>
		{
			new() { Name = "Weak", IsOnline = true, ActiveTasks = 1 },
			new() { Name = "Strong", IsOnline = true, ActiveTasks = 0 }
		};
		_stateMock.Setup(s => s.GetStats()).Returns(stats);

		_scheduler = new WeightedRoundRobinScheduler(_optionsMock.Object, _stateMock.Object);

		var result = _scheduler.GetNextSlave();
		result.Name.Should().Be("Strong");
	}

}
