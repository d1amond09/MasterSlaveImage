using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace MasterSlaveImage.Tests.Helpers;

public class MasterProcessManager(string exePath, ITestOutputHelper output) : IDisposable
{
	private Process? _process;
	private readonly string _exePath = exePath;
	private readonly ITestOutputHelper _output = output;

	public void Start()
	{
		var startInfo = new ProcessStartInfo
		{
			FileName = _exePath,
			WorkingDirectory = Path.GetDirectoryName(_exePath),
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};

		_process = Process.Start(startInfo);

		if (_process != null)
		{
			Task.Run(async () =>
			{
				while (!_process.StandardError.EndOfStream)
				{
					string? line = await _process.StandardError.ReadLineAsync();
					if (!string.IsNullOrEmpty(line)) _output.WriteLine($"[MASTER ERR] {line}");
				}
			});
		}
	}

	public void Dispose()
	{
		try
		{
			if (_process != null && !_process.HasExited)
			{
				_process.Kill();
				_process.WaitForExit();
			}
		}
		catch { }
	}
}
