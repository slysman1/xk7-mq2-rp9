// QuestDataSO.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Delivery Quest", fileName = "Quest - ")]
public class QuestDataSO : ScriptableObject
{
    public int respectPointsReward = 1;
    public List<QuestStartingItem> startingItems = new();
    public List<QuestDeliveryRequirement> deliveryRequirements = new();
    public UpgradeType neededUpgrade;

    public bool CanBeCompleted(List<Item_Base> submitted)
    {
        // Total items that requirements say we need
        int requiredTotal = deliveryRequirements.Sum(req => req.amount);

        // If counts don't match → reject early
        if (submitted.Count != requiredTotal)
            return false;

        // For each requirement, check if we have exactly the right amount of matching items
        foreach (var req in deliveryRequirements)
        {
            int matching = submitted.Count(item => RequirementSatisfied(req, item));

            if (matching != req.amount)
                return false;
        }

        Debug.Log("✅ Delivery successful!");
        return true;
    }


    private static bool RequirementSatisfied(QuestDeliveryRequirement req, Item_Base item)
    {
        if (req == null || item == null)
            return false;

        if (item.GetItemId() != req.itemData.itemId)
            return false;

        if (req.containerData != null)
        {
            if (item.currentItemHolder == null || item.currentItemHolder.atachedItemData != req.containerData)
                return false;
        }

        return true;
    }


    public string GetQuestDescription()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        string currentLine = "";

        foreach (var req in deliveryRequirements)
        {
            if (req == null || req.itemData == null)
                continue;

            string icon = req.itemData.emojiIcon;
            string containerIcon = req.containerData != null ? req.containerData.emojiIcon : "";
            string segment = $"{icon}x{req.amount}";

            // If item must be in container, add "in 📦"
            if (!string.IsNullOrEmpty(containerIcon))
                segment += $" in {containerIcon}";

            // Add space between segments
            if (currentLine.Length + segment.Length + 1 > 18)
            {
                // If this segment would exceed the limit, start new line
                sb.AppendLine(currentLine.TrimEnd());
                currentLine = "";
            }

            currentLine += segment + " ";
        }

        // Append the last line if anything remains
        if (!string.IsNullOrWhiteSpace(currentLine))
            sb.AppendLine(currentLine.TrimEnd());

        return sb.ToString().TrimEnd();
    }
}

[System.Serializable]
public class QuestDeliveryRequirement
{
    public ItemDataSO itemData;
    public int amount = 1;
    public ItemDataSO containerData;

    public QuestDeliveryRequirement(ItemDataSO itemData, int amount, ItemDataSO containerData)
    {
        this.itemData = itemData;
        this.amount = amount;
        this.containerData = containerData;
    }
}

[System.Serializable]
public class QuestStartingItem
{
    public ItemDataSO itemData;
    public int amount = 1;
}
