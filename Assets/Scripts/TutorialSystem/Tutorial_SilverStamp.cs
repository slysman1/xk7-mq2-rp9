using System.Collections.Generic;
using UnityEngine;

public class Tutorial_SilverStamp : MonoBehaviour
{
    public bool canShowSilverStampIndicator;// { get; private set; }
    public ItemDataSO silverStampData;


    public void ShowSilverStampTutorialIndicatorIfNeeded(ItemDataSO stampData, ItemDataSO coinData)
    {
        if (canShowSilverStampIndicator == false)
            return;

        bool notTryingToUseSilverStamp = stampData != silverStampData;

        if (notTryingToUseSilverStamp)
        {
            List<Tool_CoinStamp> allStamps = ItemManager.instance.FindAllItemsWithComponent<Tool_CoinStamp>();

            foreach (var stamp in allStamps)
            {
                if(stamp.GetAllowedToStampCoinData() == coinData)
                {
                    TutorialIndicator.HighlightTargetTransform(stamp.transform);
                    canShowSilverStampIndicator = false;
                }
            }
        }
    }
}
