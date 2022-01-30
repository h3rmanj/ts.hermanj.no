using Discord;
using Discord.WebSocket;

namespace ts.hermanj.no.Features
{
    public class LockVoiceChannelFeature : BotFeature
    {
        private const string LOCK = "!lock";
        private const string UNLOCK = "!unlock";

        public LockVoiceChannelFeature(ILogger<LockVoiceChannelFeature> logger, DiscordSocketClient client) : base(logger, client)
        {
        }

        public override Task Activate()
        {
            _client.MessageReceived += MessageRecieved;
            _client.UserVoiceStateUpdated += UserVoiceStateUpdated;
            return Task.CompletedTask;
        }

        private async Task MessageRecieved(SocketMessage socketMessage)
        {
            var text = socketMessage.Content.ToLower();

            if (text == LOCK || text == UNLOCK)
            {
                var textChannel = socketMessage.Channel as ITextChannel;
                var guild = _client.Guilds.FirstOrDefault(g => g.Id == textChannel?.GuildId);
                var channelName = textChannel?.Name.Replace('-', ' ');
                var channel = guild?.VoiceChannels.FirstOrDefault(vc => vc.Name == channelName);

                if (textChannel != null && channel != null)
                {
                    var tasks = new List<Task>();
                    var userLimit = text switch
                    {
                        LOCK => channel.Users.Count,
                        _ => 99
                    };

                    _logger.LogInformation("Running {command} on {channel}. Setting user limit to {limit}.", text, channelName, userLimit);

                    tasks.Add(channel.ModifyAsync(properties =>
                    {
                        properties.UserLimit = userLimit;
                    }));

                    if (text == LOCK)
                    {
                        tasks.Add(textChannel.SendMessageAsync("Channel locked to current users. Write `!unlock` to allow other users to join again."));
                    } else
                    {
                        tasks.Add(textChannel.SendMessageAsync("Channel unlocked."));
                    }

                    await Task.WhenAll(tasks);
                }
            }
        }

        private async Task UserVoiceStateUpdated(SocketUser socketUser, SocketVoiceState prevState, SocketVoiceState nextState)
        {
            // When a user leaves a channel
            if (prevState.VoiceChannel != null
                && prevState.VoiceChannel.Id != nextState.VoiceChannel?.Id
                // Channel is locked when not 99
                && prevState.VoiceChannel.UserLimit != 99)
            {
                var channel = prevState.VoiceChannel;
                var newUserLimit = channel.Users.Count switch
                {
                    0 => 99,
                    _ => channel.Users.Count
                };

                _logger.LogInformation("User left the locked channel {channel}. Updating limit to {limit}.", channel.Name, newUserLimit);

                await channel.ModifyAsync(properties =>
                {
                    properties.UserLimit = newUserLimit;
                });
            }
        }
    }
}
