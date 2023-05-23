using Models.MongoDB;

namespace Classes.Domain.Gateway
{
    public interface IDatabase
    {
        Task<SmsNotificationData> SaveNotificationAsync(SmsNotificationData data);
        Task<SmsNotificationData> GetNotificationByIdAsync(string notificationId);
        Task<bool> DeleteNotificationAsync(string notificationId);
    }
}
