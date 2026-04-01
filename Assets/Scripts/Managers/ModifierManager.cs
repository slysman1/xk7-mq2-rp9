using System.Collections.Generic;

public static class ModifierManager
{
    // % based modifiers (speed, exp, rewards)
    private static readonly Dictionary<ModifierType, float> floatModifiers =
        new Dictionary<ModifierType, float>();

    // flat bonuses (capacity, slots, limits)
    private static readonly Dictionary<ModifierType, int> intModifiers =
        new Dictionary<ModifierType, int>();

    static ModifierManager() => ResetAll();

    // ---------- GENERIC FLOAT ----------
    public static float GetMultiplier(ModifierType type)
    {
        return floatModifiers.TryGetValue(type, out var value) ? value : 1f;
    }

    // ---------- SPEED (time goes DOWN) ----------
    // percent = 0.05f for 5% faster
    public static void AddSpeedBonus(ModifierType type, float percent)
    {
        if (!floatModifiers.ContainsKey(type))
            floatModifiers[type] = 1f;

        floatModifiers[type] *= (1f - percent);
    }

    // ---------- EXP (value goes UP) ----------
    // percent = 0.05f for +5% EXP
    public static void AddExpBonus(ModifierType type, float percent)
    {
        if (!floatModifiers.ContainsKey(type))
            floatModifiers[type] = 1f;

        floatModifiers[type] *= (1f + percent);
    }

    // ---------- CARRY CAPACITY ----------
    public static int GetBonusCarryCapacity(ModifierType type)
    {
        return intModifiers.TryGetValue(type, out var value) ? value : 0;
    }

    public static void AddCarryCapacity(ModifierType type, int amount)
    {
        if (!intModifiers.ContainsKey(type))
            intModifiers[type] = 0;

        intModifiers[type] += amount;
    }

    // ---------- RESET ----------
    public static void ResetAll()
    {
        floatModifiers.Clear();
        intModifiers.Clear();

        // speed defaults
        floatModifiers[ModifierType.StampingSpeed] = 1f;
        floatModifiers[ModifierType.FurnaceSpeed] = 1f;
        floatModifiers[ModifierType.ChoppingSpeed] = 1f;

        // exp defaults
        floatModifiers[ModifierType.StampingExp] = 1f;

        // capacity defaults
        intModifiers[ModifierType.PlateCarryCapacity] = 0;
        intModifiers[ModifierType.WoodenLogCarryCapacity] = 0;
    }

    public static void AddModifier(ModifierDataSO data)
    {
        switch (data.modifierType)
        {
            // ---------- SPEED ----------
            case ModifierType.StampingSpeed:
            case ModifierType.FurnaceSpeed:
            case ModifierType.ChoppingSpeed:
            case ModifierType.CoinCutSpeed:
            case ModifierType.WaterRefillSpeed:
                AddSpeedBonus(data.modifierType, data.GetPercentModifier());
                break;

            // ---------- EXP ----------
            case ModifierType.StampingExp:
            case ModifierType.ChoppingExp:
                AddExpBonus(data.modifierType, data.GetPercentModifier());
                break;

            // ---------- CAPACITY ----------
            case ModifierType.PlateCarryCapacity:
            case ModifierType.WoodenLogCarryCapacity:
                AddCarryCapacity(data.modifierType, data.GetCapacityModifier());
                break;
        }
    }

}

public enum ModifierType
{
    // speed
    StampingSpeed,
    FurnaceSpeed,
    ChoppingSpeed,
    CoinCutSpeed,
    WaterRefillSpeed,
    

    // exp
    StampingExp,
    ChoppingExp,

    // capacity
    PlateCarryCapacity,
    WoodenLogCarryCapacity
}
