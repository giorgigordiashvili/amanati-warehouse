using Newtonsoft.Json;

namespace ServerSide.Models.ParcelService
{
    public class AuthenticationToken
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AaccessToken { get; set; } = "";

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; } = "";

        [JsonProperty(PropertyName = "expires_in")]
        public string ExpiresIn { get; set; } = "";

        [JsonProperty(PropertyName = "refresh_token")]
        public string RefreshToken { get; set; } = "";

        [JsonProperty(PropertyName = "userName")]
        public string? UserName { get; set; }

        [JsonProperty(PropertyName = ".issued")]
        public DateTime Issued { get; set; }

        [JsonProperty(PropertyName = ".expires")]
        public DateTime Expires { get; set; }
    }
}
