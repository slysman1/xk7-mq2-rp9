
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager instance;
    private OrderManager questManager => OrderManager.instance;
    private bool noNeedUpgrade => TestingManager.instance.noNeedUpgrade;

    public UpgradeDataSO currentUpgrade;
    public UpgradeDataSO[] allUpgrades;//{ get; private set; }
    private List<UpgradeDataSO> unlockedUpgrades = new List<UpgradeDataSO>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        UnlockUpgrade(currentUpgrade);
    }

    //{
    //    if (noNeedUpgrade || currentUpgrade == null)
    //        return 999;

    //    //foreach (var item in currentUpgrade.GetAllowedItemsByCategory(itemToCheck.category))
    //    //{
    //    //    if (itemToCheck.itemId == item.itemData.itemId)
    //    //        return item.allowedAmount;
    //    //}

    //    Debug.Log("Item is not in the base - " + itemToCheck.itemName);
    //    return 0;
    //} public int GetAllowedAmountOf(ItemDataSO itemToCheck)
   

    public void UnlockUpgrade(UpgradeDataSO newUpgrade)
    {
        currentUpgrade = newUpgrade;
        unlockedUpgrades.Add(newUpgrade);
        questManager.ResetQuestPool();
        //questManager.StartNewQuestSet();
    }

    public bool RequiredUpgradeUnlocked(UpgradeDataSO upgradeToCheck)
    {
        if (upgradeToCheck.requiredUpgrade == null)
            return true;

        return unlockedUpgrades.Contains(upgradeToCheck.requiredUpgrade);
    }

    public bool UpgradeUnlocked(UpgradeDataSO upgradeData) => unlockedUpgrades.Contains(upgradeData);
    public UpgradeDataSO GetUpgradeData(int upgradeIndex) => allUpgrades[upgradeIndex];
    public UpgradeDataSO GetCurrentUpgrade() => currentUpgrade;
    public UpgradeDataSO GetNextUpgradeData()
    {
        int nextIndex = currentUpgrade.GetUpgradeIndex() + 1;

        if (nextIndex >= allUpgrades.Length)
            return null;

        return allUpgrades[nextIndex];
    }

    public string GetUpgradeNameUnlockingDecor(UnlockDecorDataSO decorData)
    {
        if (decorData == null)
            return "Upgrade Was Not Found";

        foreach (var upgrade in allUpgrades)
        {
            if (upgrade == null)
                continue;

            if (upgrade.decorToUnlock.Contains(decorData))
                return upgrade.GetUpgradeName();
        }

        return "Upgrade Was Not Found";
    }
}
