using ServerSide.Models.ParcelService;

namespace ServerSide.Models
{
    public class ExchangeData
    {
        public ExchangeData()
        {
            ItemCode = string.Empty;
            Items = new List<Items>();
            FullName = string.Empty;
            GroupName = string.Empty;
        }
        public int ClientID { get; set; }
        public int Count { get; set; }
        public string ItemCode { get; set; }
        public string FullName { get; set; }
        public int RoomNumber { get; set; }
        public string GroupName { get; set; }        
        public List<Items> Items { get; set; }
        public List<GroupedParcel> GroupedParcels { get; set; }
    }
}
