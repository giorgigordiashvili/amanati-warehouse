using ServerSide.Enums;

namespace ServerSide.Models
{
    public class SendDataModel
    {
        public int ClientID { get; set; }
        public string Code { get; set; }
        public LanguageEnum Language { get; set; }        
    }
}
