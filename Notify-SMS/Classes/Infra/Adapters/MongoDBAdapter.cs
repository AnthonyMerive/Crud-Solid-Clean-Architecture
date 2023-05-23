using Microsoft.Extensions.Options;
using Models.MongoDB;
using MongoDB.Driver;
using Models.Settings;
using Classes.Domain.Gateway;

namespace Classes.Infra.Adapters
{
    public class MongoDBAdapter : IDatabase
    {
        private readonly IMongoCollection<SmsNotificationData> mongoCollection;
        private readonly ILogger<MongoDBAdapter> logger;

        public MongoDBAdapter(
            IMongoClient mongoClient,
            IOptions<Settings> settings,
            ILogger<MongoDBAdapter> logger
            )
        {
            var mongoDatabase = mongoClient.GetDatabase(settings.Value.MongoDb.Notifications.DatabaseName);
            this.mongoCollection = mongoDatabase.GetCollection<SmsNotificationData>(settings.Value.MongoDb.Notifications.CollectionName);
            this.logger = logger;
        }

        public async Task<SmsNotificationData> SaveNotificationAsync(SmsNotificationData data)
        {
            try
            {
                await mongoCollection.InsertOneAsync(data);

                return data;
            }
            catch (Exception ex)
            {
                logger.LogError(MongoDBAdapterLogs.SAVE_DOCUMENT_ERROR, ex.Message);
                return null;
            }
        }

        public async Task<SmsNotificationData> GetNotificationByIdAsync(string notificationId)
        {
            try
            {
                return await mongoCollection.FindAsync(x => x.Id.Equals(notificationId)).Result.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(MongoDBAdapterLogs.FOUND_DOCUMENT_ERROR, ex.Message);
                return null;
            }
        }

        public async Task<bool> DeleteNotificationAsync(string notificationId)
        {
            try
            {
                var deleteResult = await mongoCollection.DeleteOneAsync(x => x.Id.Equals(notificationId));

                return deleteResult.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(MongoDBAdapterLogs.DELETE_DOCUMENT_ERROR, ex.Message);
                return false;
            }

        }
    }

    internal static class MongoDBAdapterLogs
    {
        public const string SAVE_DOCUMENT_ERROR = "[Notify-SMS] Ha ocurrido un error al guardar el documento en MongoDB - ERROR: {ex.Message}";
        public const string FOUND_DOCUMENT_ERROR = "[Notify-SMS] Ha ocurrido un error al obtener el documento en MongoDB - ERROR: {ex.Message}";
        public const string DELETE_DOCUMENT_ERROR = "[Notify-SMS] Ha ocurrido un error al borrar el documento en MongoDB - ERROR: {ex.Message}";
    }
}
