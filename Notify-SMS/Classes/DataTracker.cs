using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Models.Settings;
using Models.RabbitMQ;
using Classes.Infra.Adapters;

namespace Classes
{
    public static class DataTracker
    {
        public static readonly Queue<IDictionary<string, object>> DataQueue = new();
        public static readonly ILogger Logger;

        public static async Task TrackEventAsync(object headers, object body, string operation, string traceId, Exception exception)
        {
            IDictionary<string, object> dataTrack = new Dictionary<string, object>
            {
                { "Headers", headers },
                { "Body", body },
                { "Operation", operation },
                { "TraceId", traceId },
                { "Exception", exception },
                { "Timestamp", DateTime.Now }
            };

            int counter = 0;

            do
                try
                {
                    lock (DataQueue) DataQueue.Enqueue(dataTrack);
                    break;
                }
                catch (Exception ex)
                {
                    Logger.LogError("[Notify-SMS] Error: try={counter} - Exception: {ex.Message}", counter, ex.Message);
                    counter++;
                    Thread.Sleep(100);
                }
            while (counter < 3);

            await Task.Delay(1);
        }
    }

    public class DataTrackerHostedService : BackgroundService
    {
        private readonly IQueue rabbitMQAdapter;
        private readonly TraceabilitySettings traceability;
        private readonly ILogger logger;

        public DataTrackerHostedService(
            IQueue rabbitMQAdapter,
            IOptions<Settings> settings,
            ILogger<DataTrackerHostedService> logger)
        {
            this.rabbitMQAdapter = rabbitMQAdapter;
            traceability = settings.Value.Traceability;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await DoTracking();
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }

        private async Task DoTracking()
        {
            try
            {
                await ValidateQueue();
            }
            catch (Exception ex)
            {
                logger.LogError("[Notify-SMS] Error: {GetType().Name} - {ex.Message}", GetType().Name, ex.Message);
            }
        }

        private async Task ValidateQueue()
        {
            if (DataTracker.DataQueue.Count > 0)
            {
                IDictionary<string, object> dataTrack = DataTracker.DataQueue.Dequeue();

                if (dataTrack is not null)
                {
                    var canonicalMessage = BuildCanonicalMessage(
                        dataTrack["Headers"],
                        dataTrack["Body"],
                        dataTrack["Operation"].ToString(),
                        (DateTime)dataTrack["Timestamp"],
                        dataTrack["TraceId"].ToString(),
                        (Exception)dataTrack["Exception"]);

                    await ExecuteElkTracking(canonicalMessage);
                }
            }
            else
            {
                Thread.Sleep(2);

                if (DataTracker.DataQueue.Count == 0)
                {
                    lock (DataTracker.DataQueue) DataTracker.DataQueue.TrimExcess();
                }
            }
        }

        private async Task ExecuteElkTracking(LogData canonicalMessage)
        {
            int counter = 0;

            do
            {
                if (await ElkTrackingAsync(canonicalMessage)) break;
                counter++;
                Thread.Sleep(10);

            }
            while (counter < 3);
        }

        private async Task<bool> ElkTrackingAsync(LogData canonicalMessage)
        {
            if (traceability.Enabled)
            {
                try
                {
                    var serializeMessage = JsonConvert.SerializeObject(canonicalMessage);

                    if (canonicalMessage.Status == "ERROR")
                    {
                        logger.LogError("[Notify-SMS] Sendding error to ELK, Message:{serializeMessage}", serializeMessage);
                    }
                    else
                    {
                        logger.LogInformation("[Notify-SMS] Sendding message to ELK, Message:{serializeMessage}", serializeMessage);
                    }

                    await rabbitMQAdapter.PublishMessageAsync(serializeMessage);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        private LogData BuildCanonicalMessage(
            object headers,
            object body,
            string operation,
            DateTime timestamp,
            string traceId,
            Exception exception)
        {

            string type = "OUT";
            string status = "OK";
            string trace = "Success";
            string messageResult = "Success";

            if (exception is not null)
            {
                status = "ERROR";
                trace = exception.Message;
                messageResult = exception.StackTrace;
            }

            if (operation.ToLower().Contains("request")) type = "IN";

            return new()
            {
                TransactionID = traceId,
                IntegrationName = traceability.ServiceName,
                DomainName = traceability.NameSpace,
                Operation = operation,
                Type = type,
                TimeStamp = timestamp,
                Event = new EventType
                {
                    Header = new HeaderType
                    {
                        TransactionID = traceId,
                        ApplicationID = traceability.ServiceName,
                        TransactionDate = timestamp,
                        FlexField = headers
                    },
                    Data = body
                },
                Status = status,
                Trace = trace,
                MessageResult = messageResult
            };

        }
    }
}