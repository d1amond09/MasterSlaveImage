using System.Diagnostics;
using System.Net.Sockets;
using FluentAssertions;
using MasterSlaveImage.Tests.Helpers;
using Xunit.Abstractions;

namespace MasterSlaveImage.Tests;

public class LoadTests : IDisposable
{
	private readonly ITestOutputHelper _output;

	private const string FtpHost = "127.0.0.1";
	private const int FtpPort = 21021;
	private const string FtpUser = "user";
	private const string FtpPass = "password";

	private const string MasterExePath = @$"../../../../MasterSlaveImage.Master/bin/Debug/net9.0/MasterSlaveImage.Master.exe";
	private const string SlaveExePath = @$"../../../../MasterSlaveImage.SlaveNode/bin/Debug/net9.0/MasterSlaveImage.SlaveNode.exe";

	private readonly MasterProcessManager _masterManager;

	public LoadTests(ITestOutputHelper output)
	{
		_output = output;

		string masterAbsPath = Path.GetFullPath(MasterExePath);
		string slaveAbsPath = Path.GetFullPath(SlaveExePath);

		if (!File.Exists(masterAbsPath))
			throw new FileNotFoundException($"Master exe не найден!\n{masterAbsPath}");
		if (!File.Exists(slaveAbsPath))
			throw new FileNotFoundException($"Slave exe не найден!\n{slaveAbsPath}");

		_output.WriteLine("Запуск Master Server...");
		_masterManager = new MasterProcessManager(masterAbsPath, _output);
		_masterManager.Start();

		if (!WaitForPortOpen(FtpHost, FtpPort, 10000))
		{
			throw new Exception($"Master Server не запустился за 10 секунд на порту {FtpPort}.");
		}
		_output.WriteLine("Master Server запущен и готов.");
	}

	[Theory]
	[InlineData(10)]
	[InlineData(25)]
	[InlineData(50)]
	[InlineData(75)]
	[InlineData(100)] 
	[InlineData(250)] 
	[InlineData(500)] 
	[InlineData(1000)] 
	[InlineData(2000)] 
	[InlineData(4000)] 
	public async Task Scenario1_Stability_GrowingLoad(int imageCount)
	{
		_output.WriteLine($"\n=== СЦЕНАРИЙ 1: Стабильность ({imageCount} img) ===");

		using var slaveManager = new SlaveProcessManager(4, SlaveExePath, _output);
		slaveManager.Start();

		await Task.Delay(4000);

		string zipPath = PayloadGenerator.Generate(imageCount);

		try
		{
			var sw = Stopwatch.StartNew();
			bool success = await FtpTaskExecutor.ExecuteAsync(zipPath, imageCount, FtpHost, FtpPort, FtpUser, FtpPass, _output);
			sw.Stop();

			success.Should().BeTrue();

			double fps = imageCount / sw.Elapsed.TotalSeconds;
			_output.WriteLine($"[RESULT] Count: {imageCount} | Time: {sw.Elapsed.TotalSeconds:F5}s | Speed: {fps:F5} img/sec");
		}
		finally
		{
			if (File.Exists(zipPath)) File.Delete(zipPath);
		}
	}

	[Fact]
	public async Task Scenario2_Scalability_Speedup()
	{
		_output.WriteLine("\n=== СЦЕНАРИЙ 2: Масштабируемость (50 img) ===");
        
		int imageCount = 50;
		int[] workerCounts = { 8 }; 
		double baseTime = 0;

		foreach (var wCount in workerCounts)
		{
			_output.WriteLine($"\n--- Тест: {wCount} Worker(s) ---");
			
			using var slaveManager = new SlaveProcessManager(wCount, SlaveExePath, _output);
			slaveManager.Start();
			
			await Task.Delay(4000);
			
			string zipPath = PayloadGenerator.Generate(imageCount);

			try
			{
				var sw = Stopwatch.StartNew();
				bool success = await FtpTaskExecutor.ExecuteAsync(zipPath, imageCount, FtpHost, FtpPort, FtpUser, FtpPass, _output);
				sw.Stop();

				success.Should().BeTrue();

				double time = sw.Elapsed.TotalSeconds;
                
				string speedup = "1.00x (Base)";
				if (baseTime == 0)
				{
					baseTime = time;
				}
				else
				{
					if (time < 0.01) time = 0.01; 
					double ratio = baseTime / time;
					speedup = $"{ratio:F5}x";
				}

				_output.WriteLine($"[RESULT] Workers: {wCount} | Time: {time:F5}s | Speedup: {speedup}");
			}
			finally
			{
				if (File.Exists(zipPath)) File.Delete(zipPath);
			}
		}
	}

	public void Dispose()
	{
		_output.WriteLine("Остановка Master Server...");
		_masterManager.Dispose();
	}

	private bool WaitForPortOpen(string host, int port, int timeoutMs)
	{
		var sw = Stopwatch.StartNew();
		while (sw.ElapsedMilliseconds < timeoutMs)
		{
			try
			{
				using var client = new TcpClient();
				var result = client.BeginConnect(host, port, null, null);
				var success = result.AsyncWaitHandle.WaitOne(100);
				if (success)
				{
					client.EndConnect(result);
					return true;
				}
			}
			catch { }
			Thread.Sleep(200);
		}
		return false;
	}

	private async Task Warmup()
	{
		_output.WriteLine("--- WARMUP (Прогрев) ---");
		using var sm = new SlaveProcessManager(1, SlaveExePath, _output);
		sm.Start();
		await Task.Delay(3000);
		string zip = PayloadGenerator.Generate(5);
		await FtpTaskExecutor.ExecuteAsync(zip, 5, FtpHost, FtpPort, FtpUser, FtpPass, _output);
		File.Delete(zip);
		_output.WriteLine("--- WARMUP DONE ---\n");
	}
}