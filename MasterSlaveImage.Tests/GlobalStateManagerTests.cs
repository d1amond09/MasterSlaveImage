using FluentAssertions;
using MasterSlaveImage.Master.Application.Dtos;
using MasterSlaveImage.Master.Application.Implementations;
using MasterSlaveImage.Master.Domain.Configuration;
using Microsoft.Extensions.Options;
using Moq;

namespace MasterSlaveImage.Tests;

public class GlobalStateManagerTests
{
	private readonly Mock<IOptions<MasterSettings>> _optionsMock;
	private readonly GlobalStateManager _stateManager;
	private readonly List<Master.Domain.Configuration.SlaveNode> _slaves;

	public GlobalStateManagerTests()
	{
		_slaves =
		[
			new() { Name = "Slave1", Host = "127.0.0.1", Port = 9001, Weight = 1 },
			new() { Name = "Slave2", Host = "127.0.0.1", Port = 9002, Weight = 2 }
		];

		_optionsMock = new Mock<IOptions<MasterSettings>>();
		_optionsMock.Setup(o => o.Value).Returns(new MasterSettings { Slaves = _slaves });

		_stateManager = new GlobalStateManager(_optionsMock.Object);
	}

	[Fact]
	public void Constructor_ShouldInitializeSlavesAsOffline()
	{
		var stats = _stateManager.GetStats();

		stats.Should().HaveCount(2);
		stats.All(s => !s.IsOnline).Should().BeTrue();
	}

	[Fact]
	public void GetAllSlaves_ShouldReturnConfiguredSlaves()
	{
		var result = _stateManager.GetAllSlaves();
		result.Should().BeEquivalentTo(_slaves);
	}

	[Fact]
	public void UpdateSlaveStatus_ShouldUpdateOnlineStatus()
	{
		_stateManager.UpdateSlaveStatus("Slave1", true);

		var stats = _stateManager.GetStats();
		var slave1 = stats.First(s => s.Name == "Slave1");

		slave1.IsOnline.Should().BeTrue();
	}

	[Fact]
	public void UpdateSlaveStatus_ShouldUpdateActiveTasks()
	{
		_stateManager.UpdateSlaveStatus("Slave1", true, 1);

		var stats = _stateManager.GetStats();
		var slave1 = stats.First(s => s.Name == "Slave1");

		slave1.ActiveTasks.Should().Be(1);
	}

	[Fact]
	public void UpdateSlaveStatus_ShouldIncrementTotalProcessed()
	{
		_stateManager.UpdateSlaveStatus("Slave1", true, 0, true);
		_stateManager.UpdateSlaveStatus("Slave1", true, 0, true);

		var stats = _stateManager.GetStats();
		var slave1 = stats.First(s => s.Name == "Slave1");

		slave1.TotalProcessed.Should().Be(2);
	}

	[Fact]
	public void EnqueueAndDequeue_ShouldWorkCorrectly()
	{
		var task = new QueuedTask { ResultPath = "path" };

		_stateManager.EnqueueTask(task);
		var success = _stateManager.TryDequeueTask(out var result);

		success.Should().BeTrue();
		result.Should().Be(task);
	}

	[Fact]
	public void TryDequeue_ShouldReturnFalse_WhenQueueIsEmpty()
	{
		var success = _stateManager.TryDequeueTask(out var result);
		success.Should().BeFalse();
		result.Should().BeNull();
	}

}
