using Discord.WebSocket;
using ts.hermanj.no;
using ts.hermanj.no.Features;

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
        services.AddHostedService<DiscordWorker>();
        services.AddHostedService<CustomColorFeature>();
        services.AddHostedService<VoiceTextChannelFeature>();
        //services.AddHostedService<DailyWordleFeature>();
        services.AddHostedService<LockVoiceChannelFeature>();
    })
    .Build();

await host.RunAsync();
