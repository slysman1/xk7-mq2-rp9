using System.Collections.Generic;
using UnityEngine;

public static class OrderGenerator
{
    public static void Generate(OrderDataSO order)
    {
        order.orderInput = new List<OrderInput>();
        order.orderOutput = new List<OrderOutput>();
        order.rewardOutput = new List<OrderOutput>();

        int targetCredits = Random.Range(order.minCoinsToMake, order.maxCoinsToMake + 1);

        int rewardCredits = Mathf.Max(
            1,
            Mathf.RoundToInt((float)targetCredits / order.rewardCoefficient)
        );

        int deliveryCredits = targetCredits - rewardCredits;

        int remainingCredits = targetCredits;
        int remainingDeliveryCredits = deliveryCredits;

        Dictionary<ItemDataSO, OrderInput> inputStack = new();
        Dictionary<ItemDataSO, OrderOutput> outputStack = new();
        Dictionary<ItemDataSO, OrderOutput> rewardStack = new();

        int safety = 1000;

        while (remainingCredits > 0 && safety-- > 0)
        {
            var candidate = order.allowedInputs[Random.Range(0, order.allowedInputs.Count)];

            int coinAmount = candidate.coinQuantity;

            if (coinAmount > remainingCredits)
                continue;

            remainingCredits -= coinAmount;

            // INPUT STACK
            if (!inputStack.TryGetValue(candidate.input, out var input))
            {
                input = new OrderInput
                {
                    itemData = candidate.input,
                    itemQuantity = 0,
                    coinQuantity = 0
                };

                inputStack[candidate.input] = input;
            }

            input.itemQuantity++;
            input.coinQuantity += coinAmount;

            int deliverAmount = Mathf.Min(coinAmount, remainingDeliveryCredits);

            // DELIVERY OUTPUT
            if (deliverAmount > 0)
            {
                if (!outputStack.TryGetValue(candidate.result, out var output))
                {
                    output = new OrderOutput
                    {
                        itemData = candidate.result,
                        quantity = 0
                    };

                    outputStack[candidate.result] = output;
                }

                output.quantity += deliverAmount;
                remainingDeliveryCredits -= deliverAmount;
            }

            // REWARD OUTPUT
            int rewardAmount = coinAmount - deliverAmount;

            if (rewardAmount > 0)
            {
                if (!rewardStack.TryGetValue(candidate.result, out var reward))
                {
                    reward = new OrderOutput
                    {
                        itemData = candidate.result,
                        quantity = 0
                    };

                    rewardStack[candidate.result] = reward;
                }

                reward.quantity += rewardAmount;
            }
        }

        order.orderInput.AddRange(inputStack.Values);
        order.orderOutput.AddRange(outputStack.Values);
        order.rewardOutput.AddRange(rewardStack.Values);

        int totalRewardCredits = 0;

        foreach (var reward in rewardStack.Values)
        {
            totalRewardCredits += reward.quantity * reward.itemData.creditValue;
        }

        order.totalCoinsToMake = targetCredits;
        order.totalCreditReward = totalRewardCredits;
    }
}