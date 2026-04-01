using UnityEngine;

public class UI_Upgrade : MonoBehaviour
{
    private UpgradeManager upgradeManager => UpgradeManager.instance;

    [SerializeField] private GameObject upgradeSlotPrefab;
    [SerializeField] private Transform upgradeSlotParent;
    [SerializeField] private UpgradeDataSO[] upgradeData;
    [SerializeField] private UI_UpgradeSlot[] slots;

    private void Start()
    {

        SetupSlots();
        UpdateList(null);

        UI_UpgradeSlot.OnUpgradeBuy += UpdateList;
    }

    public void UpdateList(UpgradeDataSO purchasedUpgrade)
    {
        if (slots == null || slots.Length == 0 || upgradeData == null || upgradeData.Length == 0)
            return; // nothing to update

        foreach (var slot in slots)
            slot.gameObject.SetActive(false);

        for (int i = 0; i < slots.Length; i++)
        {
            if (i == 0)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].UpdateSlot();
            }

            if (slots[i] != null && slots[i].NeededUpgradeUnlocked())
            {
                // make sure we don't access i+1 out of bounds
                if (i + 1 >= slots.Length || slots[i + 1] == null)
                    continue;

                slots[i + 1].gameObject.SetActive(true);
                slots[i + 1].UpdateSlot();
            }
        }
    }

    public void SetupSlots()
    {
        // Sync with UpgradeManager
        upgradeData = upgradeManager.allUpgrades;


        for (int i = 0; i < upgradeData.Length - 1; i++)
            Instantiate(upgradeSlotPrefab, upgradeSlotParent);

        slots = GetComponentsInChildren<UI_UpgradeSlot>(true);


        // Hide all slots first
        foreach (var slot in slots)
            slot.gameObject.SetActive(false);

        // Determine how many slots we can actually fill
        //int maxIndex = Mathf.Min(slots.Length, upgradeData.Length - 1);

        for (int i = 0; i < upgradeData.Length; i++)
        {
            var upgradeData = this.upgradeData[i];
            var slot = slots[i];

            slot.gameObject.SetActive(true);
            slot.SetupSlot(upgradeData);
        }
    }
}
