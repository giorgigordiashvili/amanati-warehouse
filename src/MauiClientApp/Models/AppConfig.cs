namespace Amanati.ge.Models
{
    public class AppConfig
    {
        public string OperatorServerIP { get; set; }
        public int ClientID { get; set; }

        public int BranchID { get; set; }        
        public string ServiceEndpoint { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public int ClientNotoficationTimeOut{ get; set; }
    }
}
