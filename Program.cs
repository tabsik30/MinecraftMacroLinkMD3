using MacroDeck.Plugin.Hosting;
using MacroLink.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MacroLink;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = MacroDeckPlugin.CreatePlugin(args)
            .UseMacroDeckLogging()
            .RegisterIntegration<PluginIntegration>();

        // MinecraftLinkService owns the WebSocket connection + reconnect loop to the
        // MacroLink Minecraft mod. Registered as a singleton so PluginIntegration
        // (variables) and the config flow can all reach the same running instance via DI.
        builder.Services.AddSingleton<MinecraftLinkService>();
        builder.Services.AddHostedService(sp => sp.GetRequiredService<MinecraftLinkService>());

        var plugin = builder.Build();
        await plugin.RunAsync();
    }
}
