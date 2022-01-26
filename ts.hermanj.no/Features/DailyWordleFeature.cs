using Discord.WebSocket;
using System.Text.RegularExpressions;
using ts.hermanj.no.Interfaces;

namespace ts.hermanj.no.Features
{
    public class DailyWordleFeature : IBotFeature
    {
        private static readonly string WORDLE_CHANNEL = "wordle";
        private static readonly int START_WORDLE = 221;
        private static readonly DateTime START_DATE = new DateTime(2022, 01, 26);

        private readonly ILogger<CustomColorFeature> _logger;
        private readonly DiscordSocketClient _client;

        public DailyWordleFeature(ILogger<CustomColorFeature> logger, DiscordSocketClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task Activate()
        {
            foreach (var guild in _client.Guilds)
            {
                var worldeChannel = guild.TextChannels.FirstOrDefault(c => c.Name == WORDLE_CHANNEL);
                if (worldeChannel != null)
                {
                    var todaysWordle = GetTodaysWordle();

                    var thread = worldeChannel.Threads.LastOrDefault();
                    if (thread != null)
                    {
                        //TODO: Errorhandle threads without numbers
                        var threadName = thread.Name;
                        var threadNumber = Int32.Parse(Regex.Match(threadName, @"\d+").Value);

                        if (todaysWordle != threadNumber && threadNumber < todaysWordle)
                        {
                            await worldeChannel.CreateThreadAsync($"{WORDLE_CHANNEL} {todaysWordle}");
                        }
                    }
                    else
                    {
                        await worldeChannel.CreateThreadAsync($"{WORDLE_CHANNEL} {todaysWordle}");
                    }
                }
            }

            await Task.Delay((int)GetMilliSecondsUntilMidnight());
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
