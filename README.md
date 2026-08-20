# MacroLink — mod Fabric

Client-side mod na Minecraft 26.2, który co ~4x/s wystawia dane gracza
przez lokalny serwer WebSocket (`ws://127.0.0.1:25599`):

```json
{
  "health": 18.0,
  "maxHealth": 20.0,
  "armor": 12,
  "hunger": 15,
  "x": 123.4, "y": 64.0, "z": -876.2,
  "dimension": "minecraft:overworld",
  "biome": "minecraft:plains",
  "timeOfDay": 13500,
  "air": 300,
  "maxAir": 300,
  "xpLevel": 7,
  "xpProgress": 0.42
}
```

## Zanim zbudujesz

To bardzo świeża wersja Minecrafta (26.x, oficjalne mapowania Mojanga,
Java 25). Numery wersji w `gradle.properties` mogą się zdezaktualizować
w ciągu tygodni — sprawdź https://fabricmc.net/develop przed buildem
i zaktualizuj w razie potrzeby:
- `minecraft_version`
- `loader_version`
- `fabric_version`
- `loom_version`

Wymagany JDK 25.

## Build

```
./gradlew build
```

Gotowy jar (z wszytą biblioteką Java-WebSocket) znajdziesz w
`build/libs/macrolink-0.1.0.jar`. Wrzuć go do folderu `mods` obok
Fabric API.

## Test bez Macro Deck 2

Zanim napiszemy plugin do Macro Deck 2, możesz sprawdzić czy dane lecą,
łącząc się dowolnym klientem WebSocket (np. rozszerzeniem do przeglądarki
albo `websocat`) pod adresem `ws://127.0.0.1:25599`.

## Co dalej

Plugin do Macro Deck 2 (C#/.NET) będzie klientem WebSocket łączącym się
pod ten adres, parsującym JSON i aktualizującym elementy na desce.
