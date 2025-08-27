namespace ServerSide.Models.ParcelService
{
    public class ParcelWithdrawRequest
    {
        public string Code { get; set; }
        public int BranchID { get; set; }
        public Array ItemIDs { get; set; }
    }
}
