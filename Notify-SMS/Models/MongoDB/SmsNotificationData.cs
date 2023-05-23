using MongoDB.Bson.Serialization.Attributes;

namespace Models.MongoDB
{
    public class SmsNotificationData
    {
        [BsonId()] public string Id { get; set; }
        [BsonElement("phoneNumber")] public string ClientPhoneNumber { get; set; }
        [BsonElement("user")] public string ClientUserName { get; set; }
        [BsonElement("message")] public string Message { get; set; }
        [BsonElement("transactionDate")] public DateTime TransactionDate { get; set; }
        [BsonElement("isSended")] public bool IsSended { get; set; }
    }
}
