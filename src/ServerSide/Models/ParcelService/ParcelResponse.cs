namespace ServerSide.Models.ParcelService
{
    public class ParcelResponse
    {
        public ParcelResponse()
        {            
            Code = string.Empty;
            Message = string.Empty;
            ItemCode = string.Empty;
            Items = new List<Items>();
            GroupedParcels = new List<GroupedParcel>();
        }
        public string ItemCode { get; set; }

        public string Code { get; set; }

        public string Message { get; set; }

        public List<Items> Items { get; set; }
        public List<GroupedParcel> GroupedParcels { get; set; }

    }

    public class Items
    {
        public Items()
        {
            TrackingNumber = string.Empty;
            PersonalNumber = string.Empty;
            FullName = string.Empty;
            OrganizationName = string.Empty;
            FlightName = string.Empty;
            Weight = string.Empty;
            UserGroupName = string.Empty;
            ShelfName = string.Empty;
        }

        public int ID { get; set; }
        public string TrackingNumber { get; set; }
        public int Code { get; set; }
        public string PersonalNumber { get; set; }
        public string FullName { get; set; }
        public string UserGroupName { get; set; }
        public string FlightName { get; set; }
        public string Weight { get; set; }
        public string ShelfName { get; set; }

        //ეს აღარ გვაქვს??
        public string OrganizationName { get; set; }
    }


    public class GroupedParcel
    {
        public GroupedParcel()
        {
            PersonalNumber = string.Empty;
            FullName = string.Empty;
            Items = new List<Items>();
        }
        public string PersonalNumber { get; set; }
        public string FullName { get; set; }

        public List<Items> Items { get; set; }

    }
}
