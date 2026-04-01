using System.Linq;
using UnityEngine;

public static class MetalConfig
{
    private static MetalConfigSO _config;
    public static MetalConfigSO Get()
    {
        if (_config == null)
            _config = Resources.Load<MetalConfigSO>("MetalConfig");
        return _config;
    }

    public static bool IsGold(Item_Base item) => Get().goldItems.Contains(item.itemData);
    public static bool IsSilver(Item_Base item) => Get().silverItems.Contains(item.itemData);
    public static bool IsCopper(Item_Base item) => Get().copperItems.Contains(item.itemData);
}