using BepInEx.Configuration;
using System.Collections.Generic;
using System.Reflection;

namespace LevelValuesAndCosmeticsScanner.Settings;

public enum HudAnchor
{
    BottomRight,
    Right
}

internal static class ModSettings
{
    public static ConfigEntry<bool> AlwaysOn { get; private set; }
    public static ConfigEntry<bool> ShowTotalValue { get; private set; }
    public static ConfigEntry<bool> ShowItemCount { get; private set; }
    public static ConfigEntry<bool> ShowCosmeticBoxes { get; private set; }
    public static ConfigEntry<HudAnchor> UIPosition { get; private set; }

    public static void Bind(ConfigFile config)
    {
        config.SaveOnConfigSet = false;

        AlwaysOn = Entry(config, "Default", "AlwaysOn", true, "Always show the tracker during a level instead of only while the map key is held.");
        ShowTotalValue = Entry(config, "Default", "ShowTotalValue", true, "Show the total value of valuables outside the extraction zone.");
        ShowItemCount = Entry(config, "Default", "ShowItemCount", true, "Show the number of valuables outside the extraction zone.");
        ShowCosmeticBoxes = Entry(config, "Default", "ShowCosmeticBoxes", true, "Scan and show compact cosmetic box counters beside the value tracker.");
        UIPosition = Entry(config, "UIPosition", "UIPosition", HudAnchor.BottomRight, "Preset tracker position.");

        RemoveUnknownEntries(config);

        config.Save();
        config.SaveOnConfigSet = true;
    }

    private static ConfigEntry<T> Entry<T>(ConfigFile config, string section, string key, T value, string description)
    {
        return config.Bind(section, key, value, description);
    }

    private static void RemoveUnknownEntries(ConfigFile config)
    {
        var property = typeof(ConfigFile).GetProperty("OrphanedEntries", BindingFlags.Instance | BindingFlags.NonPublic);
        if (property == null)
            return;

        var entries = (Dictionary<ConfigDefinition, string>)property.GetValue(config);
        entries?.Clear();
    }
}
