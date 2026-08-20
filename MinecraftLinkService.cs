using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MacroLink.Services;

/// <summary>
/// Owns the WebSocket connection to the MacroLink Minecraft mod. Replaces
/// MacroLinkClient's manual reconnect-loop thread from Macro Deck 2 with a
/// BackgroundService - the reconnect-every-5s behavior is preserved, as is the
/// original's "restart immediately after config is saved" behavior (RequestReconnect,
/// called from MacroLinkConfigFlow, mirrors Main.ReloadConnection -> client.Restart()).
///
/// Field names and behavior confirmed against the real MD2 MacroLinkClient.cs source
/// (not guessed): health/maxHealth/armor/hunger/x/y/z/dimension/biome/timeOfDay/air/
/// maxAir/xpLevel. dimension and biome carry a "minecraft:" namespace prefix that gets
/// stripped. timeOfDay is the raw game tick counter, not a 0-24000 time-of-day value
/// (same caveat the original plugin's comment carried over).
///
/// Values are kept as raw strings (JsonElement.GetRawText(), same as the original's
/// el.ToString()) rather than parsed doubles - the original plugin exposed everything
/// as VariableType.String, and doing the same here sidesteps the decimal-formatting
/// quirks we hit with ETS2's Numeric variables.
/// </summary>
public sealed class MinecraftLinkService(ILogger<MinecraftLinkService> logger) : BackgroundService
{
    private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(5);

    private ClientWebSocket? _activeSocket;
    private CancellationTokenSource _reconnectSignal = new();

    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 25599;

    public bool IsConnected { get; private set; }
    public string Health { get; private set; } = "";
    public string MaxHealth { get; private set; } = "";
    public string Armor { get; private set; } = "";
    public string Hunger { get; private set; } = "";
    public string PositionX { get; private set; } = "";
    public string PositionY { get; private set; } = "";
    public string PositionZ { get; private set; } = "";
    public string Dimension { get; private set; } = "";
    public string Biome { get; private set; } = "";
    public string GameTime { get; private set; } = "";
    public string Air { get; private set; } = "";
    public string MaxAir { get; private set; } = "";
    public string XpLevel { get; private set; } = "";

    // Numeric (not raw string, unlike the fields above) because the XP-progress
    // slider action needs an actual double for its 0-100 range, not display text.
    public double XpProgressPercent { get; private set; }

    // Called after a config flow save (host/port changed) to reconnect right away
    // instead of waiting out the current ReconnectDelay - mirrors the MD2 plugin's
    // Main.ReloadConnection() -> MacroLinkClient.Restart().
    public void RequestReconnect()
    {
        try
        {
            _activeSocket?.Abort();
        }
        catch (ObjectDisposedException)
        {
        }

        _reconnectSignal.Cancel();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var socket = new ClientWebSocket();
                _activeSocket = socket;
                await socket.ConnectAsync(new Uri($"ws://{Host}:{Port}"), stoppingToken);
                IsConnected = true;
                logger.LogInformation("Connected to MacroLink mod at {Host}:{Port}", Host, Port);

                var buffer = new byte[8192];
                await using var ms = new MemoryStream();
                while (socket.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
                {
                    ms.SetLength(0);
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await socket.ReceiveAsync(buffer, stoppingToken);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            goto disconnected;
                        }

                        ms.Write(buffer, 0, result.Count);
                    } while (!result.EndOfMessage);

                    ParseSnapshot(Encoding.UTF8.GetString(ms.ToArray()));
                }

                disconnected: ;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogDebug(ex, "MacroLink connection to {Host}:{Port} failed, will retry", Host, Port);
            }
            finally
            {
                IsConnected = false;
                _activeSocket = null;
            }

            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            // Wait ReconnectDelay, unless RequestReconnect() fires the signal early.
            var signal = Volatile.Read(ref _reconnectSignal);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, signal.Token);
            try
            {
                await Task.Delay(ReconnectDelay, linked.Token);
            }
            catch (OperationCanceledException)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                // Otherwise this was RequestReconnect() - fall through and retry now.
            }

            if (signal.IsCancellationRequested)
            {
                var fresh = new CancellationTokenSource();
                Interlocked.Exchange(ref _reconnectSignal, fresh)?.Dispose();
            }
        }
    }

    private void ParseSnapshot(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Health = ReadRaw(root, "health");
            MaxHealth = ReadRaw(root, "maxHealth");
            Armor = ReadRaw(root, "armor");
            Hunger = ReadRaw(root, "hunger");
            PositionX = ReadRaw(root, "x");
            PositionY = ReadRaw(root, "y");
            PositionZ = ReadRaw(root, "z");
            Dimension = StripNamespace(ReadString(root, "dimension"));
            Biome = StripNamespace(ReadString(root, "biome"));
            GameTime = ReadRaw(root, "timeOfDay");
            Air = ReadRaw(root, "air");
            MaxAir = ReadRaw(root, "maxAir");
            XpLevel = ReadRaw(root, "xpLevel");
            if (TryGetDouble(root, "xpProgress", out var progress))
            {
                XpProgressPercent = Math.Clamp(progress * 100.0, 0, 100);
            }
        }
        catch (JsonException ex)
        {
            logger.LogDebug(ex, "Could not parse a MacroLink message");
        }
    }

    private static bool TryGetDouble(JsonElement root, string property, out double value)
    {
        if (root.TryGetProperty(property, out var element) && element.TryGetDouble(out value))
        {
            return true;
        }

        value = default;
        return false;
    }

    private static string ReadRaw(JsonElement root, string property) =>
        root.TryGetProperty(property, out var element) ? element.GetRawText() : "";

    private static string ReadString(JsonElement root, string property) =>
        root.TryGetProperty(property, out var element) ? element.GetString() ?? "" : "";

    // "minecraft:sulfur_caves" -> "sulfur_caves"
    private static string StripNamespace(string value)
    {
        var idx = value.IndexOf(':');
        return idx >= 0 ? value[(idx + 1)..] : value;
    }
}
