using Discord;
using Discord.WebSocket;
using ts.hermanj.no.Interfaces;

namespace ts.hermanj.no.Features
{
    public class DailyWordleFeature : IBotFeature
    {
        private static readonly string WORDLE_CHANNEL = "general";
        private static readonly int START_WORDLE = 221;
        private static readonly DateTime START_DATE = new DateTime(2022, 01, 26);

        private readonly ILogger<DailyWordleFeature> _logger;
        private readonly DiscordSocketClient _client;

        public DailyWordleFeature(ILogger<DailyWordleFeature> logger, DiscordSocketClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task Activate()
        {
            // this might be bad? idc its a bot
            while (true)
            {
                try
                {
                    await CreateDailyWordleThread();
                } catch (Exception ex)
                {
                    _logger.LogInformation(ex, "what");
                }

                await Task.Delay((int)GetMilliSecondsUntilMidnight());
            }
        }
        
        private async Task CreateDailyWordleThread ()
        {
            foreach (var guild in _client.Guilds)
            {
                var wordleChannel = guild.TextChannels.FirstOrDefault(c => c.Name == WORDLE_CHANNEL);
                if (wordleChannel != null)
                {
                    var wordleThreadName = $"wordle {GetTodaysWordle()}";

                    if (!wordleChannel.Threads.Any(t => t.Name == wordleThreadName))
                    {
                        _logger.LogInformation("Creating thread");

                        var thread = await wordleChannel.CreateThreadAsync(wordleThreadName, autoArchiveDuration: ThreadArchiveDuration.ThreeDays);

                        _logger.LogInformation("Posting message in thread");

                        await (thread as IThreadChannel).SendMessageAsync("Post your daily wordle result!\n<https://www.powerlanguage.co.uk/wordle/>");
                    }
                }
            }
        }

        private static double GetMilliSecondsUntilMidnight()
        {
            var now = DateTime.Now;
            var tmr = now.AddDays(1).Date;

            return (tmr - now).TotalMilliseconds;
        }

        private static int GetTodaysWordle()
        {
            var worldeCount = (DateTime.Today - START_DATE).Days;
            return START_WORDLE + worldeCount;
        }
    }
}
