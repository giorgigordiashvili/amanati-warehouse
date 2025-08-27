namespace ServerSide.OptionsModels
{
    public class ParcelOptions
    {
        public string? BaseURL { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? BranchID { get; set; }
        public int WorkstationCount { get; set; } = 1;
    }
}
