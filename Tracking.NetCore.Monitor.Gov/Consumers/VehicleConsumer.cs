using System.Collections.Concurrent;

namespace Tracking.NetCore.Monitor.Gov.Consumers
{
    public class VehicleConsumer
    {
        private readonly RabbitMqSubcriber _rabbitMqSubcriber;
        private readonly ILogger<VehicleConsumer> _logger;
        private readonly IVehicleService _vehicleService;

        private ConcurrentDictionary<int, GovmtTransferImageReportVehicle> _govmtTransferImageReportVehicle = new();

        public VehicleConsumer(ILogger<VehicleConsumer> logger, RabbitMqSubcriberSettings rabbitMqSubcriberSettings,
            ILogger<RabbitMqSubcriber> rabbitMqLogger, IVehicleService vehicleService)
        {
            _logger = logger;
            _vehicleService = vehicleService;
            _rabbitMqSubcriber = new RabbitMqSubcriber(rabbitMqSubcriberSettings, rabbitMqLogger);
            _rabbitMqSubcriber.Received += RabbitMqSubcriberReceived;
        }

        public void Open()
        {
            _rabbitMqSubcriber.Open();
        }

        public async Task RabbitMqSubcriberReceived(RabbitMqSubcriber.ReceivedMessageArgs e)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<SendImageEvent>(Encoding.UTF8.GetString(e.Data));

                var vehicles = await _vehicleService.GetReportDataAsync(data.VehicleId, data.GpsTime, data.GpsTime);

                if (DateTime.Now.Day != DateTimeUtil.ToDateTime2010(data.GpsTime).Day)
                {
                    //save mongo
                    foreach (var item in _govmtTransferImageReportVehicle.Values)
                    {
                        await _vehicleService.Save(item);
                    }

                    //clear cache
                    _govmtTransferImageReportVehicle.Clear();
                }

                foreach (var item in vehicles)
                {
                    if (GetVehicle(item.VehicleId) == null)
                    {
                        _govmtTransferImageReportVehicle.TryAdd(item.VehicleId, new GovmtTransferImageReportVehicle
                        {
                            CompanyId = item.CompanyId,
                            VehicleId = item.VehicleId,
                            Id = item.Id,
                            LastGpsTime = item.LastGpsTime,
                            Time = item.Time,
                            Transfer = 1,
                            Waypoints = item.Waypoints,
                            LastModified = DateTime.Now,
                        });
                    }
                    else
                    {
                        _govmtTransferImageReportVehicle[item.VehicleId] = new GovmtTransferImageReportVehicle()
                        {
                            CompanyId = item.CompanyId,
                            VehicleId = item.VehicleId,
                            Id = item.Id,
                            LastGpsTime = item.LastGpsTime,
                            Time = item.Time,
                            Transfer = _govmtTransferImageReportVehicle[item.VehicleId].Transfer + 1,
                            Waypoints = item.Waypoints,
                            LastModified = DateTime.Now,
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private GovmtTransferImageReportVehicle GetVehicle(int vehicleId)
        {
            if (_govmtTransferImageReportVehicle.ContainsKey(vehicleId))
            {
                return _govmtTransferImageReportVehicle[vehicleId];
            }

            return null;
        }

        public void Close()
        {
            _rabbitMqSubcriber.Close();
        }
    }
}