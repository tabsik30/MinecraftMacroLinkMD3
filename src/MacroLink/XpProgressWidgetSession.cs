using MacroDeck.Sdk.Ui;
using MacroDeck.Ui.Model.Events;
using MacroDeck.Ui.Model.Nodes;
using MacroDeck.Ui.Model.Patches;
using MacroDeck.Ui.Model.Surfaces;
using MacroDeck.Ui.Runtime;
using MacroLink.Services;

namespace MacroLink;

internal sealed class XpProgressWidgetSession : IUiSession
{
    private readonly object _sync = new();
    private readonly MinecraftLinkService? _link;
    private readonly UiState<double> _progress;
    private readonly UiView _view;
    private bool _disposed;

    public XpProgressWidgetSession(UiSurface surface, MinecraftLinkService link, bool isPreview)
    {
        _progress = new UiState<double>(isPreview ? 0.42 : ToFraction(link.XpProgressPercent));
        _view = new UiView(surface, PluginIntegration.BuildXpProgressWidget(_progress));
        _view.Changed += OnViewChanged;
        _view.HandlerFaulted += OnViewFaulted;

        if (!isPreview)
        {
            _link = link;
            _link.XpProgressChanged += OnXpProgressChanged;
        }
    }

    public event EventHandler? Changed;

    public event EventHandler<UiSessionFaultedEventArgs>? Faulted;

    public UiTree BuildTree() => _view.Tree;

    public IReadOnlyList<UiPatch> DrainPatches() => _view.DrainPatches();

    public void Dispatch(UiEvent uiEvent) => _view.Dispatch(uiEvent);

    public ValueTask DisposeAsync()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return ValueTask.CompletedTask;
            }

            _disposed = true;
            if (_link is not null)
            {
                _link.XpProgressChanged -= OnXpProgressChanged;
            }
        }

        _view.Changed -= OnViewChanged;
        _view.HandlerFaulted -= OnViewFaulted;
        _view.Dispose();
        return ValueTask.CompletedTask;
    }

    private void OnXpProgressChanged(double percentage)
    {
        lock (_sync)
        {
            if (!_disposed)
            {
                _progress.Value = ToFraction(percentage);
            }
        }
    }

    private void OnViewChanged(object? sender, EventArgs eventArgs) =>
        Changed?.Invoke(this, EventArgs.Empty);

    private void OnViewFaulted(object? sender, UiHandlerFaultEventArgs eventArgs) =>
        Faulted?.Invoke(this, new UiSessionFaultedEventArgs(eventArgs.Exception.Message, eventArgs.Exception));

    private static double ToFraction(double percentage) => Math.Clamp(percentage / 100, 0, 1);
}
