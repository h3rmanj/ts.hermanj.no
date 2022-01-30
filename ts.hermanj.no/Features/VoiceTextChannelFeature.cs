using Discord;
using Discord.WebSocket;
using ts.hermanj.no.Interfaces;

namespace ts.hermanj.no.Features;

public class VoiceTextChannelFeature : IBotFeature
{
    private readonly ILogger<VoiceTextChannelFeature> _logger;
    private readonly DiscordSocketClient _client;

    public VoiceTextChannelFeature(ILogger<VoiceTextChannelFeature> logger, DiscordSocketClient client)
    {
        _logger = logger;
        _client = client;
    }

    public Task Activate()
    {
        _client.UserVoiceStateUpdated += UserVoiceStateUpdated;
        return Task.CompletedTask;
    }

    private async Task UserVoiceStateUpdated(SocketUser socketUser, SocketVoiceState prevState, SocketVoiceState newState)
    {
        if (prevState.VoiceChannel?.Id != newState.VoiceChannel?.Id)
        {
            // Disconnected from channel, cleanup
            if (prevState.VoiceChannel?.Category != null)
            {
                var channel = prevState.VoiceChannel;
                var roleName = channel.Name + "-text";

                IRole? role = channel.Guild.Roles.FirstOrDefault(r => r.Name == roleName);
                
                if (role != null)
                {
                    var user = channel.Guild.GetUser(socketUser.Id);

                    await user.RemoveRoleAsync(role);
                }

                if (!prevState.VoiceChannel.Users.Any())
                {
                    var textChannel = channel.Guild.TextChannels.FirstOrDefault(tc => tc.Name == channel.Name.Replace(' ', '-'));

                    if (textChannel != null)
                    {
                        await textChannel.DeleteAsync();
                    }
                }
            }

            // Connected to new channel, setup
            if (newState.VoiceChannel?.Category != null)
            {
                var channel = newState.VoiceChannel;
                var roleName = channel.Name + "-text";

                IRole? role = channel.Guild.Roles.FirstOrDefault(r => r.Name == roleName);

                if (role == null)
                {
                    role = await channel.Guild.CreateRoleAsync(roleName, isMentionable: false);
                }

                var user = channel.Guild.GetUser(socketUser.Id);

                await user.AddRoleAsync(role);

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

                        await textChannel.SendMessageAsync($"This is a private text channel for users in voice channel {channel.Name}. It will be deleted when everyone leaves.\nType `!lock` to limit the channel to current users.");
                    }
                }
            }
        }

        await Task.CompletedTask;
    }
}
