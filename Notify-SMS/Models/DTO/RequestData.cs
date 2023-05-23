using Newtonsoft.Json;

namespace Models.DTO
{
    public class RequestData
    {
        [JsonProperty("clientPhoneNumber")] public string ClientPhoneNumber { get; set; }
        [JsonProperty("username")] public string ClientUserName { get; set; }
        [JsonProperty("message")] public string Message { get; set;}
    }
}
