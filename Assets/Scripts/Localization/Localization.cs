using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public static class Localization
{ 


    private static readonly string[] tableNames =
    {
        "Tutorial_Step",
        "UpgradeName",
        "UpgradeInfo",
        "InputHelp",
        "UI",
        "Input_Help_Main_Npc",
        "Input_Help_Collectables",
        "Input_Help_Dummy",
        "Input_Help_Water_Barrel",
        "Input_Help_Furnace",
        "Input_Help_Metal_Template",
        "Input_Help_Coin_Punch",
        "Input_Help_Brakeable_Wall",
        "Input_Help_Chisel",
        "Input_Help_Order_Board",
        "Input_Help_Wooden_Log_Set",
        "Input_Help_Wood_Station",
        "Input_Help_Coin_Storage",
        "Input_Help_Torch_Web_Cleanup"
    };

    private static Dictionary<string, StringTable> keyCache = new();

    public static string GetString(string key)
    {
        if (keyCache.TryGetValue(key, out var cachedTable))
            return cachedTable.GetEntry(key)?.GetLocalizedString();

        foreach (var tableName in tableNames)
        {
            var table = LocalizationSettings.StringDatabase.GetTable(tableName);
            if (table == null) continue;

            var entry = table.GetEntry(key);
            if (entry == null) continue;

            keyCache[key] = table;
            return entry.GetLocalizedString();
        }

        return key;
    }

    public static void SetLanguage(string localeCode)
    {
        var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        if (locale != null)
            LocalizationSettings.SelectedLocale = locale;
    }

    public static string GetCurrentLanguage()
    {
        return LocalizationSettings.SelectedLocale.Identifier.Code;
    }
}
