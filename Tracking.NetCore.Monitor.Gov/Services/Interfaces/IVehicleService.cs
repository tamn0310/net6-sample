namespace Tracking.NetCore.Monitor.Gov.Services.Interfaces
{
    public interface IVehicleService
    {
        Task<IEnumerable<GovmtTransferImageReportVehicle>> GetReportDataAsync(int vehicleId, int from, int to);

        Task Save(GovmtTransferImageReportVehicle model);
    }
}