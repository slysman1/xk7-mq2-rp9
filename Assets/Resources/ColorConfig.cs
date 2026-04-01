using UnityEngine;

public static class ColorConfig
{
    private static ColorConfigSO _config;
    public static ColorConfigSO Get()
    {
        if (_config == null)
            _config = Resources.Load<ColorConfigSO>("ColorConfig");
        return _config;
    }
}