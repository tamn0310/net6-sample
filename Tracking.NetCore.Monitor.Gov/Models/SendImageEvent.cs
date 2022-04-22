namespace Tracking.NetCore.Monitor.Gov.Models
{
    public class SendImageEvent
    {
        /// Id xe
        /// </summary>
        public int VehicleId { get; set; }

        /// <summary>
        /// Thời gian nhận GPS
        /// </summary>
        public int GpsTime { get; set; }

        /// <summary>
        /// Biển số xe
        /// </summary>
        public string Plate { get; set; }

        /// <summary>
        /// Nhà cung cấp
        /// </summary>
        public string Provider { get; set; }
    }
}