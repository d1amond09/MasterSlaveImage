using MasterSlaveImage.SlaveNode;
using MasterSlaveImage.SlaveNode.Application.Implementations;
using MasterSlaveImage.SlaveNode.Application.Interfaces;
using MasterSlaveImage.SlaveNode.Domain.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var switchMappings = new Dictionary<string, string>()
{
	{ "-p", "SlaveSettings:ListenPort" },
	{ "--port", "SlaveSettings:ListenPort" },
	{ "-n", "SlaveSettings:Name" },
	{ "--name", "SlaveSettings:Name" }
};

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
	CreateHostBuilder(args, switchMappings).Build().Run();
}
catch (Exception ex)
{
	Log.Fatal(ex, "Host terminated");
}
finally
{
	Log.CloseAndFlush();
}

static IHostBuilder CreateHostBuilder(string[] args, Dictionary<string, string> mappings) =>
	Host.CreateDefaultBuilder(args)
		.ConfigureAppConfiguration((hostingContext, config) =>
		{
			config.AddCommandLine(args, mappings);
		})
		.UseSerilog((context, services, configuration) => configuration
			.ReadFrom.Configuration(context.Configuration)
			.ReadFrom.Services(services)
			.Enrich.FromLogContext()
			.WriteTo.Console()
			.WriteTo.File("logs/app-log-.txt", rollingInterval: RollingInterval.Day))
		.ConfigureServices((hostContext, services) =>
		{
			services.Configure<SlaveSettings>(hostContext.Configuration.GetSection("SlaveSettings"));
			services.AddSingleton<IImageProcessor, ImageProcessor>();
			services.AddSingleton<ITcpListenerService, TcpListenerService>();
			services.AddTransient<IClientHandler, ClientHandler>();
			services.AddHostedService<SlaveWorker>();
		});