using Discord;
using Discord.WebSocket;
using ts.hermanj.no.Interfaces;

namespace ts.hermanj.no;

public class DiscordWorker : BackgroundService
{
    private readonly ILogger<DiscordWorker> _logger;
    private readonly DiscordSocketClient _client;
    private readonly IEnumerable<IBotFeature> _features;
    private readonly string _token;

    public DiscordWorker(ILogger<DiscordWorker> logger, DiscordSocketClient client, IEnumerable<IBotFeature> features, IConfiguration configuration)
    {
        _logger = logger;
        _client = client;
        _features = features;
        _token = configuration["DiscordToken"];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.Log += Log;
        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();

        _client.Ready += Ready;
    }

    private Task Log(LogMessage msg)
    {
        _logger.LogInformation(msg.ToString());
        return Task.CompletedTask;
    }

    private Task Ready()
    {
        _logger.LogInformation("Connected to Discord.");
        
        foreach (var feature in _features)
        {
            Task.Run(() => feature.Activate());
        }
        
        return Task.CompletedTask;
    }

}