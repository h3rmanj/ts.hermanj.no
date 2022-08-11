using Discord;
using Discord.WebSocket;

namespace ts.hermanj.no.Features
{
    public class DailyWordleFeature : BotFeature
    {
        private static readonly string WORDLE_CHANNEL = "general";
        private static readonly int START_WORDLE = 221;
        private static readonly DateTime START_DATE = new(2022, 01, 26);

        public DailyWordleFeature(ILogger<DailyWordleFeature> logger, DiscordSocketClient client) : base (logger, client)
        {
        }

        public override async Task Activate()
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

                var delay = (int) GetMilliSecondsUntilMidnight();

                _logger.LogInformation("Waiting for {delay} ms to post next wordle thread", delay);

                await Task.Delay(delay);
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

                    using var loggerScope = _logger.BeginScope("Processing {WordleThreadName}", wordleThreadName);

                    if (!wordleChannel.Threads.Any(t => t.Name == wordleThreadName))
                    {
                        _logger.LogInformation("Creating thread");

                        var thread = await wordleChannel.CreateThreadAsync(wordleThreadName, autoArchiveDuration: ThreadArchiveDuration.OneDay);

                        _logger.LogInformation("Posting message in thread");

                        await (thread as IThreadChannel).SendMessageAsync("Post your daily wordle result!\n<https://www.nytimes.com/games/wordle/index.html>");
                    } else
                    {
                        _logger.LogInformation("Thread already exists");
                    }

                    _logger.LogInformation("Finding old threads to delete");

                    var messages = (await wordleChannel.GetMessagesAsync().FlattenAsync())
                        .Where(m =>
                            (m.Flags?.HasFlag(MessageFlags.HasThread) ?? false)
                            && m.Author.Id == _client.CurrentUser.Id
                            && m.CleanContent != wordleThreadName
                        );

                    _logger.LogInformation("Found {NumberOfThreadsToDelete} old wordle threads to delete", messages.Count());

                    foreach (var message in messages)
                    {
                        _logger.LogInformation("Deleting thread message {ThreadMessage}", message.CleanContent);
                        await message.DeleteAsync();
                    }
                }
            }
        }

        private static double GetMilliSecondsUntilMidnight()
        {
            var now = DateTime.Now;
            var tmr = now.AddDays(1).Date.AddSeconds(30);

            return (tmr - now).TotalMilliseconds;
        }

        private static int GetTodaysWordle()
        {
            var worldeCount = (DateTime.Today - START_DATE).Days;
            return START_WORDLE + worldeCount;
        }
    }
}
