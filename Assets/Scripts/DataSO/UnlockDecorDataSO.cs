using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Unlock Data/Decor unlock Data", fileName = "Decor unlock data - ")]
public class UnlockDecorDataSO : ScriptableObject
{
    public ItemDataSO itemData;
    public Sprite decorIcon;
    public int[] decorPricePerCopy;
    public int maxLimit;

    public int GetMaxLimit() => maxLimit;
    public int GetCurrentPrice(int amountOfItemsInTheGame)
    {
        if (LimitReached(amountOfItemsInTheGame))
            return decorPricePerCopy[decorPricePerCopy.Length - 1];//.Length - 1;

        if (amountOfItemsInTheGame >= decorPricePerCopy.Length)
            return decorPricePerCopy[decorPricePerCopy.Length - 1];
        // fallback to last price if array shorter than maxLimit

        return decorPricePerCopy[amountOfItemsInTheGame];
    }

    public  bool LimitReached(int amountOfItemsInTheGame) => amountOfItemsInTheGame >= GetMaxLimit();

    private void OnValidate()
    {
        if (maxLimit != decorPricePerCopy.Length)
            maxLimit = decorPricePerCopy.Length;
    }
}
