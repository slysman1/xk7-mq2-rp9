using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UI_Decor : MonoBehaviour
{
    private UpgradeManager upgradeManager => UpgradeManager.instance;

    [SerializeField] private GameObject decorSlotPrefab;
    [SerializeField] private Transform decorSlotParent;
    private UI_DecorSlot[] decorSlot;

    private List<UnlockDecorDataSO> unlockedDecor = new();
    private List<UnlockDecorDataSO> lockedDecor = new();

    [SerializeField] private TextMeshProUGUI totalPriceText;


    private void Awake()
    {
        decorSlot = GetComponentsInChildren<UI_DecorSlot>(true);
        UI_UpgradeSlot.OnUpgradeBuy += UnlockDecor;
    }

    public void SetupDecorIfNeeded()
    {
        foreach (var item in decorSlot)
            item.gameObject.SetActive(false);

        int index = 0;

        // Setup unlocked
        for (int i = 0; i < unlockedDecor.Count && index < decorSlot.Length; i++, index++)
        {
            decorSlot[index].gameObject.SetActive(true);
            decorSlot[index].SetupSlot(unlockedDecor[i], true);
        }

        // Setup locked
        for (int i = 0; i < lockedDecor.Count && index < decorSlot.Length; i++, index++)
        {
            decorSlot[index].gameObject.SetActive(true);
            decorSlot[index].SetupSlot(lockedDecor[i], false);
        }
    }


    public void UnlockDecor(UpgradeDataSO purchasedUpgrade)
    {
        foreach (var item in purchasedUpgrade.GetDecorToUnlock())
        {
            if(unlockedDecor.Contains(item) == false)
                unlockedDecor.Add(item);
        }

        if (upgradeManager.GetNextUpgradeData() == null)
        {
            lockedDecor.Clear();
            return;
        }

        if(upgradeManager.GetNextUpgradeData() != null)
            lockedDecor = new List<UnlockDecorDataSO>(upgradeManager.GetNextUpgradeData().decorToUnlock);

        int neededSlots = unlockedDecor.Count + lockedDecor.Count; 
        int currentSlots = decorSlot.Length;
        int slotsToCreate = Mathf.Max(0, neededSlots - currentSlots);

        for (int i = 0; i < slotsToCreate; i++)
            Instantiate(decorSlotPrefab, decorSlotParent);
        
        decorSlot = GetComponentsInChildren<UI_DecorSlot>();

    }

}
