using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Orders/Order Data")]
public class OrderDataSO : ScriptableObject
{
    public UpgradeType neededUpgradeType;
    public int favourPointReward;


    [Header("Generated Result")]
    public List<OrderInput> orderInput;
    public List<OrderOutput> orderOutput;
    public List<OrderOutput> rewardOutput;
    public int totalCoinsToMake;
    public int totalCreditReward;


    [Header("Generation Inputs")]
    public int minCoinsToMake = 10;
    public int maxCoinsToMake = 15;
    //public int minRewardCredits = 2;
    //public int maxRewardCredits = 6;
    public int rewardCoefficient = 5;
    [Space]
    public List<OrderInputItemDataSO> allowedInputs;




    public string GetOrderDescription()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (var output in orderOutput)
        {
            if (output == null || output.itemData == null)
                continue;

            string icon = output.itemData.emojiIcon;
            string segment = $"    {icon}{output.quantity}";

            sb.Append(segment);
        }

        Debug.Log(sb.ToString());
        return sb.ToString();
    }

    public bool CanBeCompleted(List<Item_Base> submitted)
    {
        int requiredTotal = orderOutput.Sum(i => i.quantity);

        if (submitted.Count != requiredTotal)
            return false;

        foreach (var output in orderOutput)
        {
            int matching = submitted.Count(item => RequirementSatisfied(output, item));

            if (matching != output.quantity)
                return false;
        }

        return true;
    }

    private static bool RequirementSatisfied(OrderOutput output, Item_Base item)
    {
        if (item.itemData == null || item == null)
            return false;

        if (output.itemData != item.itemData)
            return false;

        return true;
    }
}


[System.Serializable]
public class OrderInput
{
    public ItemDataSO itemData;
    public int itemQuantity;
    public int coinQuantity;
}

[System.Serializable]
public class OrderOutput
{
    public ItemDataSO itemData;
    public int quantity;
}