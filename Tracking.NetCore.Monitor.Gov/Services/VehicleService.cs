using Vietmap.NetCore.Legacy.MongoDB;
using Vietmap.NetCore.Legacy.MongoDB.Settings;
using Vietmap.NetCore.Legacy.Tracking.Core.Report.Proto;

namespace Tracking.NetCore.Monitor.Gov.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly ILogger<VehicleService> _logger;
        private readonly MongoDbSettings _mongoDbSettings;

        public VehicleService(ILogger<VehicleService> logger, MongoDbSettings mongoDbSettings)
        {
            _logger = logger;
            _mongoDbSettings = mongoDbSettings;
        }

        public MongoHelper GetMongoDbForBatchReport(string dataBase)
        {
            return new MongoHelper(_mongoDbSettings);
        }

        public async Task<IList<GovmtTransferImageReportVehicle>> FindExpressionAsync(int vehicleId, Expression<Func<GovmtTransferImageReportVehicle, bool>> expressions)
        {
            try
            {
                var result = GetMongoDbForBatchReport(GovmtTransferImageReportVehicle.COLLECTION).SelectAsync(VehicleReportStorageProto.GetTableByVehicleId(vehicleId), expressions).ToList();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return null;
            }
        }

        public async Task<IEnumerable<GovmtTransferImageReportVehicle>> GetReportDataAsync(int vehicleId, int from, int to)
        {
            try
            {
                return await FindExpressionAsync(vehicleId, x => x.VehicleId == vehicleId && x.Time >= from && x.Time <= to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public async Task Save(GovmtTransferImageReportVehicle model)
        {
            try
            {
               await ReplaceAsync(model,model.VehicleId, x => x.Id == model.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        private async Task ReplaceAsync(GovmtTransferImageReportVehicle obj, int vehicleId, Expression<Func<GovmtTransferImageReportVehicle, bool>> expressions)
        {
            try
            {
                var result = GetMongoDbForBatchReport(GovmtTransferImageReportVehicle.COLLECTION).Update(VehicleReportStorageProto.GetTableByVehicleId(vehicleId), obj, expressions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return;
            }
        }
    }
}