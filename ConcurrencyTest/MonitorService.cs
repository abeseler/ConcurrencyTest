
namespace ConcurrencyApp;

public sealed class MonitorService : BackgroundService
{
    private readonly QueueRepository _repo;
    private readonly PeriodicTimer _timer;
    private readonly ILogger<MonitorService> _logger;
    private readonly int _monitorId;

    public MonitorService(QueueRepository repo, ILogger<MonitorService> logger)
    {
        NumOfMonitors++;
        _monitorId = NumOfMonitors;
        _repo = repo;
        _logger = logger;
        _timer = new(TimeSpan.FromSeconds(1));
        logger.LogInformation("MonitorService created with ID: {ID}.", _monitorId);
    }

    public static int NumOfMonitors { get; set; }
    public static int RecordsToGet { get; set; }
    public static int MessageProcessingMs { get; set; } = 10;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(await _timer.WaitForNextTickAsync(stoppingToken))
        {
            if (_monitorId > NumOfMonitors || RecordsToGet <= 0)
                continue;

            var messagesProcessed = 0;
            try
            {
                var messages = (await _repo.GetNextBatch(RecordsToGet)).ToArray();
                if (messages.Length == 0)
                    continue;

                _logger.LogInformation("Monitor ({Id}) retrieved {Count} records.", _monitorId, messages.Length);
                foreach (var message in messages)
                {
                    await Task.Delay(MessageProcessingMs, stoppingToken);
                    await _repo.Delete(message.Id);
                    messagesProcessed++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Monitor ({Id}) failed to process messages. Successfully processed {Count} messages.", _monitorId, messagesProcessed);
            }
        }
    }
}
