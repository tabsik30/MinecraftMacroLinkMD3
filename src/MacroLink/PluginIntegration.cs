using System.Linq;
using MacroLink.ConfigFlow;
using MacroLink.Services;
using MacroDeck.Localization;
using MacroDeck.Sdk;
using MacroDeck.Sdk.Actions;
using MacroDeck.Sdk.ConfigFlow;
using MacroDeck.Sdk.Ui;
using MacroDeck.Sdk.Variables;
using MacroDeck.Sdk.Widgets;
using MacroDeck.Ui.Components;
using MacroDeck.Ui.Dsl;
using MacroDeck.Ui.Model.Patches;
using MacroDeck.Ui.Model.Surfaces;
using MacroDeck.Ui.Runtime;

namespace MacroLink;

/// <summary>
/// MacroLink supplies game data and a dedicated XP progress-bar widget.
/// </summary>
internal sealed class PluginIntegration(MinecraftLinkService link)
    : IPluginIntegration, IVariableProvider, IConfigFlowProvider, IWidgetTypeProvider, IUiProvider
{
    private const string XpProgressWidgetTypeId = "xp-progress";

    private static readonly WidgetTypeDescriptor XpProgressWidgetType = new(
        XpProgressWidgetTypeId,
        LocalizedText.FromLiteral("Minecraft XP Progress"),
        LocalizedText.FromLiteral("Green Minecraft experience progress bar."));

    private string? _registeredXpProgressWidgetTypeId;

    public IReadOnlyList<IActionDefinition> Actions { get; } = [];

    public async Task InitializeAsync(IIntegrationContext context)
    {
        // Read back the persisted host/port (written by MacroLinkConfigFlow.SubmitAsync
        // via ConfigFlowResult.Complete) so they survive a plugin restart.
        var entries = await context.Config.GetEntriesAsync();
        var entry = entries.Count > 0 ? entries[0] : null;
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

    public IReadOnlyList<VariableDefinition> Variables { get; } =
    [
        VariableDefinition.Eager("mc-connected", VariableType.Boolean, refreshInterval: TimeSpan.FromSeconds(1)),
        VariableDefinition.Eager("mc-health", VariableType.Text, refreshInterval: TimeSpan.FromSeconds(1)),
        VariableDefinition.Eager("mc-max-health", VariableType.Text, refreshInterval: TimeSpan.FromSeconds(1)),
        VariableDefinition.Eager("mc-armor", VariableType.Text, refreshInterval: TimeSpan.FromSeconds(1)),
        VariableDefinition.Eager("mc-hunger", VariableType.Text, refreshInterval: TimeSpan.FromSeconds(1)),
        VariableDefinition.Eager("mc-x", VariableType.Text, refreshInterval: TimeSpan.FromSeconds(1)),
        VariableDefinition.Eager("mc-y", VariableType.Text, refreshInterval: TimeSpan.FromSeconds(1)),
        VariableDefinition.Eager("mc-z", VariableType.Text, refreshInterval: TimeSpan.FromSeconds(1)),
        VariableDefinition.Eager("mc-dimension", VariableType.Text, refreshInterval: TimeSpan.FromSeconds(1)),
        VariableDefinition.Eager("mc-biome", VariableType.Text, refreshInterval: TimeSpan.FromSeconds(1)),
        VariableDefinition.Eager("mc-game-time", VariableType.Text, refreshInterval: TimeSpan.FromSeconds(1)),
        VariableDefinition.Eager("mc-air", VariableType.Text, refreshInterval: TimeSpan.FromSeconds(1)),
        VariableDefinition.Eager("mc-max-air", VariableType.Text, refreshInterval: TimeSpan.FromSeconds(1)),
        VariableDefinition.Eager("mc-xp-level", VariableType.Text, refreshInterval: TimeSpan.FromSeconds(1)),
        VariableDefinition.Eager("mc-target-name", VariableType.Text, refreshInterval: TimeSpan.FromSeconds(1)),
        VariableDefinition.Eager("mc-target-type", VariableType.Text, refreshInterval: TimeSpan.FromSeconds(1)),
        VariableDefinition.Eager("mc-xp-progress", VariableType.Numeric, decimalPlaces: 0, refreshInterval: TimeSpan.FromSeconds(1)),
    ];

    public ValueTask<VariableReading> ReadAsync(string localId, CancellationToken cancellationToken)
    {
        VariableReading reading = localId switch
        {
            "mc-connected" => VariableReading.Of(link.IsConnected),
            "mc-health" => VariableReading.Of(link.Health),
            "mc-max-health" => VariableReading.Of(link.MaxHealth),
            "mc-armor" => VariableReading.Of(link.Armor),
            "mc-hunger" => VariableReading.Of(link.Hunger),
            "mc-x" => VariableReading.Of(link.PositionX),
            "mc-y" => VariableReading.Of(link.PositionY),
            "mc-z" => VariableReading.Of(link.PositionZ),
            "mc-dimension" => VariableReading.Of(link.Dimension),
            "mc-biome" => VariableReading.Of(link.Biome),
            "mc-game-time" => VariableReading.Of(link.GameTime),
            "mc-air" => VariableReading.Of(link.Air),
            "mc-max-air" => VariableReading.Of(link.MaxAir),
            "mc-xp-level" => VariableReading.Of(link.XpLevel),
            "mc-target-name" => VariableReading.Of(link.TargetName),
            "mc-target-type" => VariableReading.Of(link.TargetType),
            "mc-xp-progress" => VariableReading.Of(Math.Round(link.XpProgressPercent), 0, 100, 1),
            _ => VariableReading.Unavailable,
        };

        return ValueTask.FromResult(reading);
    }

    public ValueTask<VariableWriteResult> SetValueAsync(
        string localId,
        object? value,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(VariableWriteResult.NotWritable());
    }

    public IReadOnlyList<WidgetTypeDescriptor> GetWidgetTypes() => [XpProgressWidgetType];

    async Task IWidgetTypeProvider.InitializeAsync(
        IWidgetTypeProviderContext context,
        CancellationToken cancellationToken)
    {
        var registration = await context.RegisterWidgetTypeAsync(XpProgressWidgetType, cancellationToken);
        _registeredXpProgressWidgetTypeId = registration.WidgetTypeId;
    }

    public IReadOnlyList<UiSurfaceDeclaration> Surfaces { get; } =
    [
        new() { Kind = UiSurfaceKinds.Widget, SessionMode = UiSessionModes.Shared },
        new() { Kind = UiSurfaceKinds.Preview, SessionMode = UiSessionModes.Shared },
    ];

    public Task<IUiSession?> CreateSessionAsync(UiSessionRequest request, CancellationToken cancellationToken)
    {
        if (request.Surface.Kind is not (UiSurfaceKinds.Widget or UiSurfaceKinds.Preview) ||
            _registeredXpProgressWidgetTypeId is null ||
            !request.Surface.Attributes.TryGetValue(UiWidgetSurfaceAttributes.WidgetType, out var widgetType) ||
            widgetType.GetString() != _registeredXpProgressWidgetTypeId)
        {
            return Task.FromResult<IUiSession?>(null);
        }

        var isPreview = request.Surface.Kind == UiSurfaceKinds.Preview;
        IUiSession session = new XpProgressWidgetSession(request.Surface, link, isPreview);
        return Task.FromResult<IUiSession?>(session);
    }

    internal static UiStack BuildXpProgressWidget(UiState<double> progress) => new()
    {
        Key = "xp-progress",
        Justify = UiComponentJustify.Center,
        Gap = 0.04,
        Padding = 0.05,
        Children =
        [
            new UiTextRun
            {
                Key = "label",
                Text = "XP",
                Size = 0.12,
                Role = UiComponentTextRoles.Secondary,
                Align = UiComponentAlignments.Center,
            },
            new UiRangeBar
            {
                Key = "bar",
                Fill = true,
                Thickness = 0.12,
                Start = 0,
                End = UiValue.From(() => progress.Value),
                StartColor = "#35C759",
                EndColor = "#35C759",
            },
            new UiTextRun
            {
                Key = "percentage",
                Text = UiText.From(() => $"{progress.Value:P0}"),
                Size = 0.16,
                Align = UiComponentAlignments.Center,
            },
        ],
    };

    // --- IConfigFlowProvider ---

    public IConfigFlow CreateConfigFlow() => new MacroLinkConfigFlow(link);
}
