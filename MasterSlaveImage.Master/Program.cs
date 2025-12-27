using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem;
using MasterSlaveImage.Master;
using MasterSlaveImage.Master.Application.Implementations;
using MasterSlaveImage.Master.Application.Interfaces;
using MasterSlaveImage.Master.Domain.Configuration;
using MasterSlaveImage.Master.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

Log.Logger = new LoggerConfiguration()
	.WriteTo.Console()
	.WriteTo.File(
		path: "logs/log-.txt",       
		rollingInterval: RollingInterval.Day, 
		retainedFileCountLimit: 7,
		outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
	)
	.CreateBootstrapLogger();

try
{
	await Host.CreateDefaultBuilder(args)
		.UseSerilog((context, services, configuration) => configuration
			.ReadFrom.Configuration(context.Configuration)
			.ReadFrom.Services(services)
			.Enrich.FromLogContext()
			.WriteTo.Console()
			.WriteTo.File("logs/app-log-.txt", rollingInterval: RollingInterval.Day))
		.ConfigureServices((c, services) =>
		{
			var config = c.Configuration.GetSection("MasterSettings");
			services.Configure<MasterSettings>(config);
			var masterSettings = config.Get<MasterSettings>();

			services.AddFtpServer(builder => builder
				.UseDotNetFileSystem()
				.EnableAnonymousAuthentication() 
			);

			services.AddSingleton<IMembershipProvider, SimpleMembershipProvider>();
			services.AddSingleton<IFileSystemClassFactory, CustomFileSystemFactory>();

			services.Configure<FtpServerOptions>(opt =>
			{
				opt.ServerAddress = "127.0.0.1";
				opt.Port = masterSettings?.Ftp?.Port ?? 21;
			});

			services.AddSingleton<ICustomFtpService, CustomFtpServer>();

			services.AddSingleton<IGlobalState, GlobalStateManager>();
			services.AddSingleton<ITaskProcessor, TaskProcessor>();
			services.AddSingleton<ISlaveScheduler, WeightedRoundRobinScheduler>();
			services.AddSingleton<ISlaveCommunicator, SlaveCommunicator>();
			services.AddSingleton<IFileWatcherService, FileWatcherService>();

			services.AddHostedService<HealthCheckService>();
			services.AddHostedService<MasterWorker>();
			services.AddHostedService<DispatcherService>();
			services.AddHostedService<StatusWriterService>();
		})
		.Build()
		.RunAsync();
}
catch (Exception ex)
{
	Log.Fatal(ex, "Fail");
}
finally
{
	Log.CloseAndFlush();
}