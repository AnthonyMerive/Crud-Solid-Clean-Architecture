using Models.DTO;
using Models.MongoDB;
using Classes.Domain.Gateway;
using Classes.Domain.Commons.Utils;
using Newtonsoft.Json;
using Classes.Domain.Commons.Ex;

namespace Classes.Domain
{
    public class NotifyService : INotifyService
    {
        private readonly IDatabase database;

        public NotifyService(IDatabase mongoDBAdapter)
        {
            this.database = mongoDBAdapter;
        }

        public async Task<ResponseData> PostSMSNotificationAsync(RequestData requestData)
        {
            var traceId = Guid.NewGuid().ToString();

            await DataTracker.TrackEventAsync(
               new object(),
               JsonConvert.SerializeObject(requestData),
               TraceabilityUtils.GetOperationIn(nameof(PostSMSNotificationAsync)),
               traceId,
               null);

            var savedNotification = await database.SaveNotificationAsync(MapNotification(requestData));

            if (savedNotification is null)
            {
                await DataTracker.TrackEventAsync(
                  new object(),
                  JsonConvert.SerializeObject(requestData),
                  TraceabilityUtils.GetOperationOut(nameof(PostSMSNotificationAsync)),
                  traceId,
                  new BusinessException(ResponseErrors.SAVE_ERROR));

                return ResponseWithError(ResponseErrors.SAVE_ERROR, traceId);
            }

            await DataTracker.TrackEventAsync(
                new object(),
                JsonConvert.SerializeObject(savedNotification),
                TraceabilityUtils.GetOperationOut(nameof(PostSMSNotificationAsync)),
                traceId,
                null);

            return SuccessResponse(savedNotification, traceId, null);
        }

        public async Task<ResponseData> GetSMSNotificationByIdAsync(string notificationId)
        {
            var traceId = Guid.NewGuid().ToString();

            await DataTracker.TrackEventAsync(
               new object(),
               notificationId,
               TraceabilityUtils.GetOperationIn(nameof(GetSMSNotificationByIdAsync)),
               traceId,
               null);

            var gettedNotification = await database.GetNotificationByIdAsync(notificationId);

            if (gettedNotification is null)
            {
                await DataTracker.TrackEventAsync(
                  new object(),
                  notificationId,
                  TraceabilityUtils.GetOperationOut(nameof(GetSMSNotificationByIdAsync)),
                  traceId,
                  new BusinessException(ResponseErrors.GET_ERROR));

                return ResponseWithError(ResponseErrors.GET_ERROR, traceId);
            }

            await DataTracker.TrackEventAsync(
               new object(),
               JsonConvert.SerializeObject(gettedNotification),
               TraceabilityUtils.GetOperationOut(nameof(GetSMSNotificationByIdAsync)),
               traceId,
               null);

            return SuccessResponse(gettedNotification, traceId, null);
        }

        public async Task<ResponseData> DeleteSMSNotificationAsync(string notificationId)
        {
            var traceId = Guid.NewGuid().ToString();

            await DataTracker.TrackEventAsync(
               new object(),
               notificationId,
               TraceabilityUtils.GetOperationIn(nameof(DeleteSMSNotificationAsync)),
               traceId,
               null);

            var notificationToDelete = await database.GetNotificationByIdAsync(notificationId);

            if (notificationToDelete is null)
            {
                await DataTracker.TrackEventAsync(
                  new object(),
                  notificationId,
                  TraceabilityUtils.GetOperationOut(nameof(DeleteSMSNotificationAsync)),
                  traceId,
                  new BusinessException(ResponseErrors.DELETE_ERROR_NO_EXIST));

                return ResponseWithError(ResponseErrors.DELETE_ERROR_NO_EXIST, traceId);
            }

            if (notificationToDelete.IsSended)
            {
                await DataTracker.TrackEventAsync(
                  new object(),
                  notificationId,
                  TraceabilityUtils.GetOperationOut(nameof(DeleteSMSNotificationAsync)),
                  traceId,
                  new BusinessException(ResponseErrors.MESSAGE_SENDDED_ERROR));

                return SuccessResponse(notificationToDelete, traceId, ResponseErrors.MESSAGE_SENDDED_ERROR);
            }

            var isNotificationDeleted = await database.DeleteNotificationAsync(notificationId);

            if (!isNotificationDeleted)
            {
                await DataTracker.TrackEventAsync(
                  new object(),
                  notificationId,
                  TraceabilityUtils.GetOperationOut(nameof(DeleteSMSNotificationAsync)),
                  traceId,
                  new BusinessException(ResponseErrors.DELETE_ERROR));

                return ResponseWithError(ResponseErrors.DELETE_ERROR, traceId);
            }

            await DataTracker.TrackEventAsync(
               new object(),
               isNotificationDeleted,
               TraceabilityUtils.GetOperationOut(nameof(DeleteSMSNotificationAsync)),
               traceId,
               null);

            return SuccessResponse(notificationToDelete, traceId, null);
        }

        private static SmsNotificationData MapNotification(RequestData requestData)
        {
            return new SmsNotificationData
            {
                Id = Guid.NewGuid().ToString(),
                ClientPhoneNumber = requestData.ClientPhoneNumber,
                ClientUserName = requestData.ClientUserName,
                Message = requestData.Message,
                TransactionDate = DateTime.UtcNow,
                IsSended = false
            };
        }

        private static ResponseData ResponseWithError(string error, string traceId)
        {
            return new ResponseData
            {
                Message = MessageResponses.ERROR_MESSAGE,
                Errors = new[] { error ?? string.Empty },
                TraceId = traceId,
                Data = null
            };
        }

        private static ResponseData SuccessResponse(SmsNotificationData data, string traceId, string error)
        {
            return new ResponseData
            {
                Message = MessageResponses.SUCCESS_MESSAGE,
                Errors = new[] { error },
                TraceId = traceId,
                Data = new Data
                {
                    NotifyId = data.Id,
                    ClientPhoneNumber = data.ClientPhoneNumber,
                    SendStatus = !data.IsSended ? MessageResponses.MESSAGE_SENDDED : MessageResponses.MESSAGE_NOT_SENDDED
                }
            };
        }
    }

    internal static class ResponseErrors
    {
        public const string SAVE_ERROR = "Error guardando la notificacion de la base de datos";
        public const string GET_ERROR = "Error obteniendo la notificacion de la base de datos";
        public const string DELETE_ERROR = "Error borrando la notificacion de la base de datos";
        public const string DELETE_ERROR_NO_EXIST = "Error la notificacion no existe";
        public const string MESSAGE_SENDDED_ERROR = "El mensaje que intenta eliminar ya fue enviado";
    }

    internal static class MessageResponses
    {
        public const string SUCCESS_MESSAGE = "Operacion realizada con exito";
        public const string ERROR_MESSAGE = "Operacion fallida";
        public const string MESSAGE_SENDDED = "Mensaje no enviado";
        public const string MESSAGE_NOT_SENDDED = "Mensaje enviado";
    }

    public interface INotifyService
    {
        Task<ResponseData> PostSMSNotificationAsync(RequestData requestData);
        Task<ResponseData> GetSMSNotificationByIdAsync(string notificationId);
        Task<ResponseData> DeleteSMSNotificationAsync(string notificationId);
    }
}
