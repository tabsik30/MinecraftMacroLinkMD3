package com.example.macrolink;

import net.minecraft.client.player.LocalPlayer;
import net.minecraft.core.BlockPos;
import net.minecraft.core.Holder;
import net.minecraft.resources.ResourceKey;
import net.minecraft.world.level.biome.Biome;

import java.util.Locale;
import java.util.Optional;

public class PlayerDataCollector {

    public static String collect(LocalPlayer player) {
        BlockPos pos = player.blockPosition();

        // Nazwa biomu - biom to Holder<Biome>, jego "klucz rejestru" daje nam
        // ResourceLocation typu "minecraft:plains", "minecraft:sulfur_caves" itd.
        String biomeName = "unknown";
        Holder<Biome> biomeHolder = player.level().getBiome(pos);
        Optional<ResourceKey<Biome>> biomeKey = biomeHolder.unwrapKey();
        if (biomeKey.isPresent()) {
            // UWAGA: od 1.21.11 ResourceLocation -> Identifier, wiec zakladam
            // ze accessor tez zmienil nazwe na identifier(). Jesli to sie nie
            // skompiluje, to jest jedyna linijka do sprawdzenia w IDE.
            biomeName = biomeKey.get().identifier().toString();
        }

        String dimension = player.level().dimension().identifier().toString();
        // TYMCZASOWO: getDayTime() zniknal bo w 26.1 doszedl system "world clock".
        // getGameTime() to podstawowy licznik trybow gry (tick od startu swiata) -
        // nie jest to "pora dnia 0-24000", ale powinien dzialac od razu.
        // Prawdziwy dzien/noc dorobimy jak znajdziemy wlasciwe wejscie do world clock API.
        long timeOfDay = player.level().getGameTime();

        int air = player.getAirSupply();
        int maxAir = player.getMaxAirSupply();
        int xpLevel = player.experienceLevel;
        // Postep do nastepnego levelu, 0.0-1.0 - to wlasnie to pole napedza pasek
        // doswiadczenia pod paskiem hotbaru w standardowym HUD-zie Minecrafta.
        float xpProgress = player.experienceProgress;

        return String.format(Locale.US,
                "{"
                        + "\"health\":%.1f,"
                        + "\"maxHealth\":%.1f,"
                        + "\"armor\":%d,"
                        + "\"hunger\":%d,"
                        + "\"x\":%.2f,"
                        + "\"y\":%.2f,"
                        + "\"z\":%.2f,"
                        + "\"dimension\":\"%s\","
                        + "\"biome\":\"%s\","
                        + "\"timeOfDay\":%d,"
                        + "\"air\":%d,"
                        + "\"maxAir\":%d,"
                        + "\"xpLevel\":%d,"
                        + "\"xpProgress\":%.3f"
                        + "}",
                player.getHealth(),
                player.getMaxHealth(),
                player.getArmorValue(),
                player.getFoodData().getFoodLevel(),
                player.getX(), player.getY(), player.getZ(),
                dimension,
                biomeName,
                timeOfDay,
                air,
                maxAir,
                xpLevel,
                xpProgress
        );
    }
}
