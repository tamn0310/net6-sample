namespace Tracking.NetCore.Monitor.Gov.Models
{
    [ProtoContract]
    [BsonIgnoreExtraElements]
    public class GovmtTransferImageReportVehicle
    {
        public const string COLLECTION = "tbl_VietmapGovmtTransferImageReport";

        [JsonProperty(PropertyName = "_id")]
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; }

        [ProtoMember(1)]
        public int CompanyId { get; set; }

        [ProtoMember(2)]
        public int VehicleId { get; set; }

        [ProtoMember(3)]
        public int Time { get; set; }

        [ProtoMember(4)]
        public string Plate { get; set; }

        [ProtoMember(5)]
        public int Transfer { get; set; }

        [ProtoMember(6)]
        public int Received { get; set; }

        [ProtoMember(7)]
        public int Run4H { get; set; }

        [ProtoMember(10)]
        public DateTime LastModified { get; set; } = DateTime.Now;

        [ProtoMember(8)]
        public string Type { get; set; }

        [ProtoMember(9)]
        public string Manage { get; set; }

        [ProtoMember(10)]
        public string Company { get; set; }

        [ProtoMember(11)]
        public int LastGpsTime { get; set; } = 0;

        [ProtoMember(12)]
        public int Waypoints { get; set; }
    }
}