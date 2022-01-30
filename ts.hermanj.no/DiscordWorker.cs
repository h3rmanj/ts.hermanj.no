using Discord;
using Discord.WebSocket;

namespace ts.hermanj.no;

public class DiscordWorker : BackgroundService
{
    private readonly ILogger<DiscordWorker> _logger;
    private readonly DiscordSocketClient _client;
    private readonly string _token;

    public DiscordWorker(ILogger<DiscordWorker> logger, DiscordSocketClient client, IConfiguration configuration)
    {
        _logger = logger;
        _client = client;
        _token = configuration["DiscordToken"];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.Log += Log;
        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();
    }

    private Task Log(LogMessage msg)
    {
        _logger.LogInformation(msg.ToString());
        return Task.CompletedTask;
    }
}