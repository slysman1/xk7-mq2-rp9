
using System.Collections.Generic;
using UnityEngine;

public class Item_ClothBanner : Item_Base
{
    [SerializeField] private BannerUpgrade[] bannerUpgrades;
    private List<Mesh> meshVariants = new List<Mesh>();
    private int bannerIndex = 0;

    protected override void Start()
    {
        base.Start();

        UI_UpgradeSlot.OnUpgradeBuy += UpdateBanner;
    }


    private void UpdateBanner(UpgradeDataSO upgradeData)
    {
        foreach (var upgrade in bannerUpgrades)
        {
            if (upgrade.neededUpgradeData == upgradeData)
            {
                meshFilter.mesh = upgrade.bannerMesh;
                meshVariants.Add(upgrade.bannerMesh);
            }
        }
    }

    public override void SeconderyInteraction(Transform caller = null)
    {
        base.SeconderyInteraction(caller);

        CycleMesh();
    }

    private void CycleMesh()
    {
        if (meshVariants == null || meshVariants.Count == 0)
            return;

        bannerIndex++;
        bannerIndex %= meshVariants.Count;

        meshFilter.mesh = meshVariants[bannerIndex];
    }

    public override void ShowInputUI(bool enable)
    {
        if (enable)
        {
            if (inventory.CanPickup(this))
            {
                if (itemData.pickupType == PickupType.Hold)
                    inputHelp.AddInput(KeyType.LMB_Hold, "input_pickup_hold");
                else
                    inputHelp.AddInput(KeyType.LMB, "input_pickup_click");
            }

            if (meshVariants.Count > 1)
                inputHelp.AddInput(KeyType.F, "input_help_switch_banner");
        }
        else
            inputHelp.RemoveInput();
    }
}

[System.Serializable]
public class BannerUpgrade
{
    public UpgradeDataSO neededUpgradeData;
    public Mesh bannerMesh;
}