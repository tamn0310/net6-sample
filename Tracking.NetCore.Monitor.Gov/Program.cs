using Tracking.NetCore.Monitor.Gov.Consumers;
using Tracking.NetCore.Monitor.Gov.Services;
using Vietmap.NetCore.Legacy.MongoDB.Settings;

const string Debug = "debug";
const string File = "file";
const string Udp = "udp";

try
{
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var slash = environment == "Production" ? "/" : @"\";

    var configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables()
                            .Build();

    Log.Logger = new LoggerConfiguration()
     .Enrich.FromLogContext()
     .Enrich.WithThreadId()
     .Enrich.WithThreadName()
     .Enrich.WithMachineName()
     .WriteTo.Async(a =>
     {
         var sinks = configuration["Serilog:Sink"].Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(a => a.ToLowerInvariant()).ToList();
         var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} <{ThreadId}><{ThreadName}> [{Level:u3}] - {Message:lj}{NewLine}{Exception}";

         a.Console();
         if (sinks.Contains(Debug))
         {
             a.Debug(outputTemplate: outputTemplate);
         }
         if (sinks.Contains(File))
         {
             a.File($"Logs{(false ? "/" : "\\")}log-.txt", rollingInterval: RollingInterval.Day,
                 rollOnFileSizeLimit: true, fileSizeLimitBytes: 102400,
                 outputTemplate: outputTemplate);
         }
         if (sinks.Contains(Udp))
         {
             a.Udp(configuration["Serilog:Host"], int.Parse(configuration["Serilog:Port"]), AddressFamily.InterNetwork,
                 outputTemplate: outputTemplate);
         }
     }, bufferSize: 500)
     .Enrich.WithProperty("Environment", environment)
     .ReadFrom.Configuration(configuration)
     .CreateLogger();

    Log.Logger.Information(environment);

    var rabbitSub = configuration.GetRequiredSection("RabbitMQ").Get<RabbitMqSubcriberSettings>();
    var mongoSettingCfg = configuration.GetRequiredSection("MongoDb").Get<MongoDbSettings>();

    var serviceProvider = new ServiceCollection()
      .AddLogging(cfg => cfg.AddSerilog())
      .AddTransient<IVehicleService, VehicleService>()
      .BuildServiceProvider();

    var logger = serviceProvider.GetService<ILogger<Program>>();

    //var mongo = new MongoContext(mongoSettingCfg);

    var logVehicleService = serviceProvider.GetService<ILogger<VehicleService>>();
    var vehicleService = new VehicleService(logVehicleService, mongoSettingCfg);

    var loggerRabbitMqSub = serviceProvider.GetService<ILogger<RabbitMqSubcriber>>();

    var serviceVehicleSub = serviceProvider.GetService<VehicleConsumer>();
    var loggerVehicleSub = serviceProvider.GetService<ILogger<VehicleConsumer>>();

    var vehicleSub = new VehicleConsumer(loggerVehicleSub, rabbitSub, loggerRabbitMqSub, vehicleService);

    try
    {
        vehicleSub.Open();
        Console.ReadLine();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, ex.Message);
    }
    finally
    {
        vehicleSub.Close();
    }
    logger.LogInformation("Gateway stopped...");
}
catch (Exception)
{
}