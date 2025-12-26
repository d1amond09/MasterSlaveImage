using System.Diagnostics;
using Xunit.Abstractions;

namespace MasterSlaveImage.Tests.Helpers;

public class SlaveProcessManager(int count, string exePath, ITestOutputHelper output) : IDisposable
{
	private readonly List<Process> _processes = [];
	private readonly int _count = count;
	private readonly string _exePath = exePath;
	private readonly ITestOutputHelper _output = output;

	public void Start()
	{
		int basePort = 9001;
		for (int i = 0; i < _count; i++)
		{
			int port = basePort + i;
			var startInfo = new ProcessStartInfo
			{
				FileName = _exePath,
				Arguments = $"-p {port} -n Slave_{port}",
				WorkingDirectory = Path.GetDirectoryName(_exePath),
				UseShellExecute = false,
				CreateNoWindow = true
			};

			var p = Process.Start(startInfo);
			if (p != null) _processes.Add(p);
		}
		_output.WriteLine($"Запущено {_count} SlaveNode.");
	}

	public void Dispose()
	{
		foreach (var p in _processes)
		{
			try { if (!p.HasExited) { p.Kill(); p.WaitForExit(); } } catch { }
		}
		_processes.Clear();
	}
}