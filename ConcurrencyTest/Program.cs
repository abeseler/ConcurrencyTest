using ConcurrencyApp;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<DbConnector>();
builder.Services.AddSingleton<QueueRepository>();

builder.Services.AddHostedService<ProducerService>();
builder.Services.AddSingleton<IHostedService, MonitorService>();
builder.Services.AddSingleton<IHostedService, MonitorService>();
builder.Services.AddSingleton<IHostedService, MonitorService>();
builder.Services.AddSingleton<IHostedService, MonitorService>();
builder.Services.AddSingleton<IHostedService, MonitorService>();
builder.Services.AddSingleton<IHostedService, MonitorService>();
builder.Services.AddSingleton<IHostedService, MonitorService>();
builder.Services.AddSingleton<IHostedService, MonitorService>();
builder.Services.AddSingleton<IHostedService, MonitorService>();
builder.Services.AddSingleton<IHostedService, MonitorService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("monitor", (int numOfMonitors, int recordsToGet, int processingTimeMs, ILogger<Program> logger) =>
{
    MonitorService.NumOfMonitors = numOfMonitors;
    MonitorService.RecordsToGet = recordsToGet;
    MonitorService.MessageProcessingMs = processingTimeMs;
    logger.LogInformation("Monitor count set to {NumOfMonitors}", numOfMonitors);
    logger.LogInformation("Monitor records to get set to {RecordsToGet}", recordsToGet);
    logger.LogInformation("Monitor message processing time set to {ProcessingTimeMs}", processingTimeMs);
    return TypedResults.Ok();
});

app.MapPost("producer", (int recordsToInsert, ILogger<Program> logger) =>
{
    ProducerService.RecordsToInsert = recordsToInsert;
    logger.LogInformation("Producer records to insert set to {RecordsToInsert}", recordsToInsert);
    return TypedResults.Ok();
});

app.Run();
