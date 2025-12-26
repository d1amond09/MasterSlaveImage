using MasterSlaveImage.Master.Application.Dtos;
using MasterSlaveImage.Master.Application.Interfaces;
using MasterSlaveImage.Master.Domain.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MasterSlaveImage.Master.Application.Implementations;

public class DispatcherService(
	ILogger<DispatcherService> logger,
	IGlobalState state,
	ISlaveCommunicator communicator,
	ISlaveScheduler scheduler) : BackgroundService
{
	private readonly ILogger<DispatcherService> _logger = logger;
	private readonly IGlobalState _state = state;
	private readonly ISlaveCommunicator _communicator = communicator;
	private readonly ISlaveScheduler _scheduler = scheduler;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("DISPATCHER: Запущен. Ожидание задач...");

		while (!stoppingToken.IsCancellationRequested)
		{
			if (_state.TryDequeueTask(out var task))
			{
				var slave = _scheduler.GetNextSlave();

				if (slave != null)
				{
					_ = ProcessTaskAsync(slave, task);
				}
				else
				{
					_logger.LogWarning("Нет доступных воркеров. Ожидание...");
					_state.EnqueueTask(task);
					await Task.Delay(1000, stoppingToken);
				}
			}
			else
			{
				await Task.Delay(100, stoppingToken);
			}
		}
	}

	private async Task ProcessTaskAsync(SlaveNode slave, QueuedTask task)
	{
		_state.UpdateSlaveStatus(slave.Name, true, activeDelta: 1);

		try
		{
			var response = await _communicator.SendTaskAsync(slave, task.Request);

			if (response.Success && response.ProcessedImageBytes != null)
			{
				string ext = task.Request.Format.ToLower();
				string nameNoExt = Path.GetFileNameWithoutExtension(task.Request.OriginalFileName);
				string savePath = Path.Combine(task.ResultPath, $"{nameNoExt}.{ext}");

				await File.WriteAllBytesAsync(savePath, response.ProcessedImageBytes);

				_state.UpdateSlaveStatus(slave.Name, true, activeDelta: -1, incrementTotal: true);
			}
			else
			{
				_logger.LogError($"Ошибка обработки на {slave.Name}: {response.ErrorMessage}");
				_state.UpdateSlaveStatus(slave.Name, true, activeDelta: -1);
			}
		}
		catch (Exception)
		{
			_logger.LogError($"Связь потеряна с {slave.Name} во время выполнения.");
			_state.UpdateSlaveStatus(slave.Name, false, activeDelta: -1);

			// Опционально: вернуть задачу в очередь (Requeue), если требуется гарантированное выполнение
			// _state.EnqueueTask(task);
		}
	}
}