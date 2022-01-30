using Discord;
using Discord.WebSocket;

namespace ts.hermanj.no.Features;

public class VoiceTextChannelFeature : BotFeature
{

    public VoiceTextChannelFeature(ILogger<VoiceTextChannelFeature> logger, DiscordSocketClient client) : base(logger, client)
    {
    }

    public override Task Activate()
    {
        _client.UserVoiceStateUpdated += UserVoiceStateUpdated;
        return Task.CompletedTask;
    }

    private async Task UserVoiceStateUpdated(SocketUser socketUser, SocketVoiceState prevState, SocketVoiceState newState)
    {
        if (prevState.VoiceChannel?.Id != newState.VoiceChannel?.Id)
        {
            await Task.WhenAll(
                Cleanup(socketUser, prevState),
                Setup(socketUser, newState));
        }
    }

    private static async Task Setup(SocketUser socketUser, SocketVoiceState newState)
    {
        // Connected to new channel, setup
        if (newState.VoiceChannel?.Category != null)
        {
            var channel = newState.VoiceChannel;
            var role = await GetOrCreateChannelRole(channel);

            await Task.WhenAll(
                AssignChannelRole(channel, socketUser, role),
                EnsureTextChannelExists(channel, role));
        }
    }

    private static async Task Cleanup(SocketUser socketUser, SocketVoiceState prevState)
    {
        // Disconnected from channel, cleanup
        if (prevState.VoiceChannel?.Category != null)
        {
            var channel = prevState.VoiceChannel;

            await Task.WhenAll(
                RemoveChannelRole(channel, socketUser),
                DeleteTextChannelIfEmpty(channel));
        }
    }

    private static async Task<IRole> GetOrCreateChannelRole(SocketVoiceChannel channel)
    {
        var roleName = channel.Name + "-text";

        IRole? role = channel.Guild.Roles.FirstOrDefault(r => r.Name == roleName);

        if (role == null)
        {
            role = await channel.Guild.CreateRoleAsync(roleName, isMentionable: false);
        }

        return role;
    }

    private static async Task AssignChannelRole(SocketVoiceChannel channel, SocketUser socketUser, IRole role)
    {
        var user = channel.Guild.GetUser(socketUser.Id);

        await user.AddRoleAsync(role);
    }

    private static async Task EnsureTextChannelExists(SocketVoiceChannel channel, IRole role)
    {
        if (!channel.Guild.TextChannels.Any(tc => tc.Name == channel.Name.Replace(' ', '-')))
        {
            var botRole = channel.Guild.CurrentUser.Roles.FirstOrDefault(role => role.Id != channel.Guild.EveryoneRole.Id);
            if (botRole != null)
            {
                var textChannel = await channel.Guild.CreateTextChannelAsync(channel.Name, properties =>
                {
                    properties.CategoryId = channel.CategoryId;
                    properties.PermissionOverwrites = new Overwrite[]
                    {
                                new Overwrite(channel.Guild.EveryoneRole.Id, PermissionTarget.Role, new OverwritePermissions().Modify(viewChannel: PermValue.Deny)),
                                new Overwrite(role.Id, PermissionTarget.Role, new OverwritePermissions().Modify(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow)),
                                new Overwrite(botRole.Id, PermissionTarget.Role, new OverwritePermissions().Modify(viewChannel: PermValue.Allow, manageChannel: PermValue.Allow, sendMessages: PermValue.Allow))
                    };
                });

                await textChannel.SendMessageAsync($"This is a private text channel for users in voice channel {channel.Name}. It will be deleted when everyone leaves.\nType `!lock` to limit the voice channel to current users.");
            }
        }
    }

    private static async Task RemoveChannelRole(SocketVoiceChannel channel, SocketUser socketUser)
    {
        var roleName = channel.Name + "-text";

        IRole? role = channel.Guild.Roles.FirstOrDefault(r => r.Name == roleName);

        if (role != null)
        {
            var user = channel.Guild.GetUser(socketUser.Id);

            await user.RemoveRoleAsync(role);
        }
    }

    private static async Task DeleteTextChannelIfEmpty(SocketVoiceChannel channel)
    {
        if (!channel.Users.Any())
        {
            var textChannel = channel.Guild.TextChannels.FirstOrDefault(tc => tc.Name == channel.Name.Replace(' ', '-'));

            if (textChannel != null)
            {
                await textChannel.DeleteAsync();
            }
        }
    }
}
