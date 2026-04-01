 using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade Data/Upgrade Data", fileName = "Upgrade data - ")]
public class UpgradeDataSO : ScriptableObject
{
    public int upgradeCost = 100;
    public UpgradeType upgradeType;
    public string upgradeNameKey;
    public string upgradeInfoKey;
    public Sprite upgradeBackground;

    public UpgradeDataSO requiredUpgrade;

    public List<ItemDataSO> equipmentToUnlock;
    public List<QuestDataSO> onUpgradeQuestDrop;
    public UnlockDecorDataSO[] decorToUnlock;
 

    public UnlockDecorDataSO[] GetDecorToUnlock() => decorToUnlock;
    public string GetUpgradeName() => Localization.GetString(upgradeNameKey);
    public string GetUpgradeDescription() => Localization.GetString(upgradeInfoKey);
    public int GetUpgradeIndex() => ((int)upgradeType);
    public string GetRequiredUpgradeName() => requiredUpgrade != null ? requiredUpgrade.GetUpgradeName() : "";
  

#if UNITY_EDITOR
    private void OnValidate()
    {
        string newName = $"Upgrade data - {upgradeType.ToString()}";

        // Rename the asset if its name doesn’t match
        string path = AssetDatabase.GetAssetPath(this);
        if (!string.IsNullOrEmpty(path))
        {
            string currentName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (currentName != newName)
            {
                AssetDatabase.RenameAsset(path, newName);
                AssetDatabase.SaveAssets();
            }
        }
    }
#endif
}


[System.Serializable]
public struct UpgradeItemsAllowed
{
    public ItemDataSO itemData;
    public int allowedAmount;
}



