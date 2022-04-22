namespace Tracking.NetCore.Monitor.Gov.Services.Interfaces
{
    public interface IMongoContext
    {
        IMongoCollection<GovmtTransferImageReportVehicle> GovmtTransferImageReportVehicles { get; }
    }
}