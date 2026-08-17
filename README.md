# MacroLink for Minecraft — MD3

## Jak to wstawić do prawdziwego projektu

1. `dotnet new macrodeck-plugin -n MacroLink --pluginId com.tabsik12.minecraft-macrolink --pluginName "MacroLink for Minecraft"`
2. Podmień wygenerowane `manifest.json`, `Program.cs`, `PluginIntegration.cs`
3. Dorzuć `Services/`, `ConfigFlow/`, `Assets/icon.svg`
4. Sprawdź w `manifest.json` czy `entrypoints.win-x64.executable` zgadza się z rzeczywistą nazwą `.exe` po `dotnet publish` (jeśli nazwiesz projekt inaczej niż `MacroLink`, popraw tę linię — tak jak przy ETS2)

## Co jest pewne

- Cała architektura (Program.cs, IPluginIntegration, IVariableProvider, IConfigFlowProvider) — sprawdzona na ETS2
- Nazwy pól WebSocket (`health`, `maxHealth`, `armor`, `hunger`, `x`/`y`/`z`, `dimension`, `biome`, `timeOfDay`, `air`, `maxAir`, `xpLevel`) oraz ucinanie namespace'u z `dimension`/`biome` — potwierdzone z prawdziwego kodu MD2 (`MacroLinkClient.cs`), nie zgadywane
- Domyślny host `127.0.0.1`, port `25599` — z oryginalnego `MacroLinkConfigurator.cs`
- Natychmiastowy restart połączenia po zapisaniu configu (`RequestReconnect()`) — odtwarza `Main.ReloadConnection()` → `client.Restart()` z oryginału

## Świadome uproszczenia względem oryginału

- Wszystkie wartości liczbowe (`health`, `armor`, `x`/`y`/`z` itd.) są typu **Text**, nie
  Numeric — dokładnie jak w MD2 (`VariableType.String` w `VariableManager.SetValue`), i
  dodatkowo unika to problemów z wymuszonym formatowaniem dziesiętnym, które mieliśmy
  przy ETS2
- `timeOfDay` zachowuje tę samą uwagę co oryginał: to surowy tick gry, nie godzina 0-24000
- Dodałem `mc_connected` (bool) — czy WebSocket jest aktualnie połączony; tego nie było
  w MD2, ale wydaje się przydatne do np. pokazania statusu połączenia na kafelku

## Test

Po buildzie i instalacji: uruchom Minecraft z modem MacroLink, sprawdź `/_macrodeck/diagnostics`
na porcie pluginu (jak przy ETS2) żeby potwierdzić `status: Connected`, potem sprawdź czy
`mc_health` itd. faktycznie się aktualizują. Pamiętaj o restarcie Macro Decka po instalacji —
beta cache'uje deklaracje capability i formatowanie do czasu restartu.
