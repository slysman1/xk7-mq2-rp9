using UnityEngine;


[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Cut Coin", fileName = "Tutorial Step - Cut Coin")]


public class TutorialStep_CutCoins : TutorialStep
{
    [SerializeField] private int coinsToCut = 2;
    private int coinsCut;

    public override void HandleTask()
    {
        coinsCut++;

        if (coinsCut >= coinsToCut)
            Complete();
    }

    public override void StartTask()
    {
        base.StartTask();
        coinsCut = 0;
        Workstation_CoinCut.OnCoinCut += HandleTask;
        TutorialIndicator.HighlightTarget<Workstation_CoinCut>();
        TutorialIndicator.HighlightTarget<Item_CoinTemplate>();

        UpdateCurrentGoalUI();
    }

    public override void StopTask()
    {
        Workstation_CoinCut.OnCoinCut -= HandleTask;
        TutorialIndicator.Clear();
    }

    public override void UpdateCurrentGoalUI()
    {
        string text = $"{Localization.GetString("tutorial_step_cut_coins")}: {coinsCut}/{coinsToCut}";

        UI.instance.inGameUI.UpdateCurrentGoal(text);
    }
}
