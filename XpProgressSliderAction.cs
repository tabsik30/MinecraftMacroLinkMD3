using MacroDeck.Sdk.Actions;
using MacroLink.Services;

namespace MacroLink.Actions;

/// <summary>
/// MD3 has no way to bind a variable directly to a slider widget - sliders are
/// always action-driven (ISliderActionDefinition, the same mechanism the built-in
/// music player uses for its volume slider). This one is read-only: dragging it
/// doesn't send anything back to Minecraft (there's no sensible "set my XP progress
/// to X" operation), it just snaps back to the real value on the next refresh.
/// </summary>
internal sealed class XpProgressSliderAction(MinecraftLinkService link) : IActionDefinition, ISliderActionDefinition
{
    public string Id => "xp-progress";
    public string Name => "XP progress";
    public string Description => "Shows progress toward the next XP level (0-100%). Read-only - dragging it doesn't change anything in-game.";

    public IReadOnlyList<ActionParameter> Parameters { get; } =
    [
        ActionParameter.Slider("value", min: 0, max: 100, label: "XP progress", step: 1),
    ];

    public string SliderValueParameter => "value";

    public Task<SliderActionState?> GetSliderStateAsync(IReadOnlyDictionary<string, object?> parameters, CancellationToken cancellationToken) =>
        Task.FromResult<SliderActionState?>(new SliderActionState(0, 100, 1, link.XpProgressPercent));

    public IActionExecutor CreateExecutor() => new Executor();

    private sealed class Executor : IActionExecutor
    {
        // Intentionally a no-op - see the class-level remarks.
        public Task<ActionResult> ExecuteAsync(ActionExecutionContext context) => ActionResult.SucceededTask;
    }
}
