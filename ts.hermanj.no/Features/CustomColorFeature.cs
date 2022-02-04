using Discord;
using Discord.WebSocket;

namespace ts.hermanj.no.Features;

public class CustomColorFeature : BotFeature
{

    private static readonly string COLOR_CHANNEL = "color";
    private static readonly ColorRole[] COLORS = new[]
    {
        new ColorRole(new Emoji("❤"), "red", Color.Red),
        new ColorRole(new Emoji("🧡"), "orange", Color.Orange),
        new ColorRole(new Emoji("💛"), "yellow", Color.Gold),
        new ColorRole(new Emoji("💚"), "green", Color.Green),
        new ColorRole(new Emoji("💙"), "blue", Color.Blue),
        new ColorRole(new Emoji("💜"), "purple", Color.Purple),
        new ColorRole(new Emoji("🖤"), "black", Color.DarkerGrey),
        new ColorRole(new Emoji("🤎"), "brown", Color.DarkOrange),
        new ColorRole(new Emoji("🤍"), "white", Color.LighterGrey),
        new ColorRole(new Emoji("🍑"), "ass", new Color(0xDD7E66)),
        new ColorRole(Emote.Parse("<:creamy:934568251378253836>"), "weeb", new Color(0xFF00D6))
    };

    public CustomColorFeature(ILogger<CustomColorFeature> logger, DiscordSocketClient client) : base(logger, client)
    {
    }

    public override async Task Activate()
    {
        foreach (var guild in _client.Guilds)
        {
            var colorChannel = guild.TextChannels.FirstOrDefault(c => c.Name == COLOR_CHANNEL);

            // TODO:
            // Automate creation of color channel

            if (colorChannel != null)
            {
                var messages = colorChannel.GetMessagesAsync(limit: 1);
                IMessage? message = (await messages.FirstOrDefaultAsync())?.FirstOrDefault();

                if (message == null)
                {
                    _logger.LogInformation("Sending color message.");
                    message = await colorChannel.SendMessageAsync("Want a custom color on this server? React to this message!");
                }
                else
                {
                    _logger.LogInformation("Removing all reactions to color message.");
                    await message.RemoveAllReactionsAsync();
                }

                await Task.WhenAll(COLORS.Select(async (color) =>
                {
                    if (!guild.Roles.Any(r => r.Name == color.Role))
                    {
                        _logger.LogInformation("Adding role {role}.", color.Role);
                        await guild.CreateRoleAsync(color.Role, color: color.Color, isMentionable: false);
                    }

                    _logger.LogInformation("Adding reaction {emote} to color message.", color.Emote.Name);
                    await message.AddReactionAsync(color.Emote);
                }));
            }
        }

        _client.ReactionAdded += ReactionAdded;
    }

    public async Task ReactionAdded(Cacheable<IUserMessage, ulong> cacheableMessage, Cacheable<IMessageChannel, ulong> cacheableChannel, SocketReaction reaction)
    {
        var message = await cacheableMessage.GetOrDownloadAsync();
        var channel = (await cacheableChannel.GetOrDownloadAsync()) as SocketTextChannel;

        if (channel?.Name == COLOR_CHANNEL && reaction.UserId != _client.CurrentUser.Id)
        {
            var tasks = new List<Task>();
            var color = COLORS.FirstOrDefault(c => c.Emote.Equals(reaction.Emote));

            if (color != null)
            {
                var role = channel.Guild.Roles.FirstOrDefault(r => r.Name == color.Role);
                var user = channel.Guild.GetUser(reaction.UserId);

                var rolesToRemove = user.Roles
                    .Where(r => COLORS.Any(c => c.Role == r.Name))
                    .Select(r => r.Id);

                tasks.Add(user.AddRoleAsync(role));

                if (rolesToRemove.Any())
                {
                    tasks.Add(user.RemoveRolesAsync(rolesToRemove));
                }
            }

            tasks.Add(message.RemoveReactionAsync(reaction.Emote, reaction.UserId));

            await Task.WhenAll(tasks);
        }
    }
}

public record ColorRole(IEmote Emote, string Role, Color Color);
