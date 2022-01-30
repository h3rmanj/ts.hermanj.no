using Discord.WebSocket;
using ts.hermanj.no;
using ts.hermanj.no.Features;
using ts.hermanj.no.Interfaces;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder =>
    {
        var configuration = builder.Build();

        if (configuration["ENVIRONMENT"] == "Development")
        {
            builder.AddUserSecrets("c46c1711-11d3-492a-b5b5-117f9085b43f");
        }
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton(new DiscordSocketClient());
        services.AddSingleton<IBotFeature, CustomColorFeature>();
        services.AddSingleton<IBotFeature, VoiceTextChannelFeature>();
        services.AddSingleton<IBotFeature, DailyWordleFeature>();
        services.AddSingleton<IBotFeature, LockVoiceChannelFeature>();
        services.AddHostedService<DiscordWorker>();
    })
    .Build();

await host.RunAsync();
