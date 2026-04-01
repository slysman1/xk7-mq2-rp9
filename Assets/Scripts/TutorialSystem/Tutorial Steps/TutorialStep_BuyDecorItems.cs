using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Buy Decor", fileName = "Tutorial Step - Buy Decor")]

public class TutorialStep_BuyDecorItems : TutorialStep
{
    [SerializeField] private int decorToBuy;
    private int decorBought;

    public override void StartTask()
    {
        base.StartTask();

        decorBought = 0;
        UI_DecorSlot.OnDecorPurchase += HandleTask;
    }


    public override void HandleTask()
    {
        decorBought++;

        if(decorBought >= decorToBuy)
            Complete();
    }

    public override void StopTask()
    {
        UI_DecorSlot.OnDecorPurchase -= HandleTask;
    }

    public override void UpdateCurrentGoalUI()
    {
        string text = $"{Localization.GetString("tutorial_step_buy_decor_items")}: {decorBought}/{decorToBuy}";
        UI.instance.inGameUI.UpdateCurrentGoal(text);
    }
}
