using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.FileSystem.DotNet;
using MasterSlaveImage.Master.Domain.Configuration;
using Microsoft.Extensions.Options;

namespace MasterSlaveImage.Master.Application.Implementations;

public class CustomFileSystemFactory : IFileSystemClassFactory
{
	private readonly string _rootPath;

	public CustomFileSystemFactory(IOptions<MasterSettings> settings)
	{
		_rootPath = Path.GetFullPath(settings.Value.Paths.FtpRoot);
		Directory.CreateDirectory(_rootPath);
		Directory.CreateDirectory(Path.Combine(_rootPath, "uploads"));
		Directory.CreateDirectory(Path.Combine(_rootPath, "results"));
	}

	public Task<IUnixFileSystem> Create(IAccountInformation accountInformation)
	{
		var fs = new DotNetFileSystem(_rootPath, false);
		return Task.FromResult<IUnixFileSystem>(fs);
	}
}