
using Dapper;

namespace ConcurrencyApp;

public sealed class ProducerService(QueueRepository repo, ILogger<ProducerService> logger) : BackgroundService
{
    private readonly PeriodicTimer Timer = new(TimeSpan.FromSeconds(1));
    public static int RecordsToInsert { get; set; }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await Timer.WaitForNextTickAsync(stoppingToken))
        {
            if (RecordsToInsert <= 0)
                continue;

            await repo.Insert(RecordsToInsert);
        }
    }
}
