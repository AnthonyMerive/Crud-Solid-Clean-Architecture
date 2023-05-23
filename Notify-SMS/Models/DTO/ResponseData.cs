using Newtonsoft.Json;

namespace Models.DTO
{
    public class ResponseData
    {
        [JsonProperty("message", Order = 1)] public string Message { get; set; }
        [JsonProperty("errors", Order = 2)] public object[] Errors { get; set; }
        [JsonProperty("traceId", Order = 3)] public string TraceId { get; set; }
        [JsonProperty("data", Order = 4)] public Data Data { get; set; }
    }

    public class Data
    {
        [JsonProperty("notifyId")] public string NotifyId { get; set; }
        [JsonProperty("phoneNumber")] public string ClientPhoneNumber { get; set; }
        [JsonProperty("sendStatus")] public string SendStatus { get; set; }
    }
}
