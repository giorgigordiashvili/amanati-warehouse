namespace ServerSide.Models
{
    public class ConnectedUser
    {
        public string? UserType { get; set; }
        public string? CustomUserID { get; set; }
        public string ConnectionID { get; set; } = "";
    }
}
