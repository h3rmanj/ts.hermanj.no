using Discord.WebSocket;

namespace ts.hermanj.no
{
    public abstract class BotFeature: BackgroundService
    {
        protected readonly DiscordSocketClient _client;
        protected readonly ILogger _logger;

        private bool IsReady = false;

        protected BotFeature(ILogger logger, DiscordSocketClient client)
        {
            _logger = logger;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _client.Ready += Ready;

            // Do it this way, so that the Ready event listener don't crash silently without the Host knowing
            while (!IsReady)
            {
                await Task.Delay(1000, stoppingToken);
            }

            await Activate();
        }

        private Task Ready()
        {
            IsReady = true;
            return Task.CompletedTask;
        }

        public abstract Task Activate();
    }
}
