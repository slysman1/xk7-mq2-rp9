using UnityEngine;

[CreateAssetMenu(menuName = "Shop Data/Modifire Upgrade Data", fileName = "Modifire Upgrade Data - ")]

public class ModifierDataSO : ScriptableObject
{
    public ModifierType modifierType;
    public float modifvierValue;
    public int expCost;
    public Sprite modifierIcon;


    public float GetPercentModifier() => modifvierValue;
    public int GetCapacityModifier() => Mathf.RoundToInt(modifvierValue);
}
