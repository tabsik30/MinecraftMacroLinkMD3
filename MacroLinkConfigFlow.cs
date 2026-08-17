using MacroLink.Services;
using MacroDeck.Sdk.Actions;
using MacroDeck.Sdk.ConfigFlow;

namespace MacroLink.ConfigFlow;

internal sealed class MacroLinkConfigFlow(MinecraftLinkService link) : IConfigFlow
{
    private const string StepId = "settings";
    private const string HostFieldName = "host";
    private const string PortFieldName = "port";

    public Task<ConfigFlowResult> StartAsync(IConfigFlowContext context, CancellationToken cancellationToken)
    {
        var step = new ConfigFlowStep
        {
            StepId = StepId,
            Title = "MacroLink connection settings",
            Description = "Where the MacroLink Minecraft mod is listening.",
            Fields =
            [
                ActionParameter.Text(HostFieldName, label: "Host", defaultValue: link.Host, required: true),
                ActionParameter.Number(PortFieldName, label: "Port", min: 1, max: 65535, defaultValue: link.Port, required: true),
            ],
        };

        return Task.FromResult(ConfigFlowResult.Step(step));
    }

    public Task<ConfigFlowResult> SubmitAsync(
        string stepId,
        IReadOnlyDictionary<string, object?> input,
        IConfigFlowContext context,
        CancellationToken cancellationToken)
    {
        var host = input.TryGetValue(HostFieldName, out var hostRaw) ? hostRaw?.ToString() : null;
        var port = input.TryGetValue(PortFieldName, out var portRaw) && portRaw is not null
            ? Convert.ToInt32(portRaw)
            : link.Port;

        if (!string.IsNullOrWhiteSpace(host))
        {
            link.Host = host;
        }

        link.Port = port;
        link.RequestReconnect();

        var values = new Dictionary<string, ConfigFlowValue>
        {
            [HostFieldName] = ConfigFlowValue.Plain(link.Host),
            [PortFieldName] = ConfigFlowValue.Plain(link.Port.ToString()),
        };

        return Task.FromResult(ConfigFlowResult.Complete("MacroLink", values));
    }
}
