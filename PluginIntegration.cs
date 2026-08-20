using System.Linq;
using MacroLink.Actions;
using MacroLink.ConfigFlow;
using MacroLink.Services;
using MacroDeck.Sdk;
using MacroDeck.Sdk.Actions;
using MacroDeck.Sdk.ConfigFlow;
using MacroDeck.Sdk.Variables;

namespace MacroLink;

/// <summary>
/// The plugin's single integration. MacroLink was a pure data feeder under Macro
/// Deck 2 (no PluginAction, only variables) - XpProgressSliderAction is the one
/// exception, added because MD3 sliders can only be action-driven, not bound
/// directly to a variable (see the class remarks there).
/// </summary>
internal sealed class PluginIntegration(MinecraftLinkService link)
    : IPluginIntegration, IVariableProvider, IConfigFlowProvider
{
    public IReadOnlyList<IActionDefinition> Actions { get; } =
    [
        new XpProgressSliderAction(link),
    ];

    public async Task InitializeAsync(IIntegrationContext context)
    {
        // Read back the persisted host/port (written by MacroLinkConfigFlow.SubmitAsync
        // via ConfigFlowResult.Complete) so they survive a plugin restart.
        var entries = await context.Config.GetEntriesAsync();
        var entry = entries.FirstOrDefault();
        if (entry is null)
        {
            return;
        }

        var host = await context.Config.GetStringAsync(entry.Id, "host");
        if (!string.IsNullOrWhiteSpace(host))
        {
            link.Host = host;
        }

        var portText = await context.Config.GetStringAsync(entry.Id, "port");
        if (int.TryParse(portText, out var port))
        {
            link.Port = port;
        }
    }

    public Task ShutdownAsync() => Task.CompletedTask;

    // --- IVariableProvider ---

    public IReadOnlyList<ProvidedVariable> ProvidedVariables { get; } =
    [
        new ProvidedVariable("mc_connected", VariableType.Boolean, RefreshInterval: TimeSpan.FromSeconds(1)),
        new ProvidedVariable("mc_health", VariableType.Text, RefreshInterval: TimeSpan.FromSeconds(1)),
        new ProvidedVariable("mc_max_health", VariableType.Text, RefreshInterval: TimeSpan.FromSeconds(1)),
        new ProvidedVariable("mc_armor", VariableType.Text, RefreshInterval: TimeSpan.FromSeconds(1)),
        new ProvidedVariable("mc_hunger", VariableType.Text, RefreshInterval: TimeSpan.FromSeconds(1)),
        new ProvidedVariable("mc_x", VariableType.Text, RefreshInterval: TimeSpan.FromSeconds(1)),
        new ProvidedVariable("mc_y", VariableType.Text, RefreshInterval: TimeSpan.FromSeconds(1)),
        new ProvidedVariable("mc_z", VariableType.Text, RefreshInterval: TimeSpan.FromSeconds(1)),
        new ProvidedVariable("mc_dimension", VariableType.Text, RefreshInterval: TimeSpan.FromSeconds(1)),
        new ProvidedVariable("mc_biome", VariableType.Text, RefreshInterval: TimeSpan.FromSeconds(1)),
        new ProvidedVariable("mc_game_time", VariableType.Text, RefreshInterval: TimeSpan.FromSeconds(1)),
        new ProvidedVariable("mc_air", VariableType.Text, RefreshInterval: TimeSpan.FromSeconds(1)),
        new ProvidedVariable("mc_max_air", VariableType.Text, RefreshInterval: TimeSpan.FromSeconds(1)),
        new ProvidedVariable("mc_xp_level", VariableType.Text, RefreshInterval: TimeSpan.FromSeconds(1)),
        new ProvidedVariable("mc_xp_progress_percent", VariableType.Text, RefreshInterval: TimeSpan.FromSeconds(1)),
    ];

    public Task<object?> GetValueAsync(string name, CancellationToken cancellationToken)
    {
        object? value = name switch
        {
            "mc_connected" => link.IsConnected,
            "mc_health" => link.Health,
            "mc_max_health" => link.MaxHealth,
            "mc_armor" => link.Armor,
            "mc_hunger" => link.Hunger,
            "mc_x" => link.PositionX,
            "mc_y" => link.PositionY,
            "mc_z" => link.PositionZ,
            "mc_dimension" => link.Dimension,
            "mc_biome" => link.Biome,
            "mc_game_time" => link.GameTime,
            "mc_air" => link.Air,
            "mc_max_air" => link.MaxAir,
            "mc_xp_level" => link.XpLevel,
            "mc_xp_progress_percent" => Math.Round(link.XpProgressPercent).ToString("0"),
            _ => null,
        };

        return Task.FromResult(value);
    }

    // --- IConfigFlowProvider ---

    public IConfigFlow CreateConfigFlow() => new MacroLinkConfigFlow(link);
}
