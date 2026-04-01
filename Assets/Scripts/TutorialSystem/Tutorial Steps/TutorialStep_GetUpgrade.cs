using UnityEngine;


[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Buy Upgrade", fileName = "Tutorial Step - Buy Upgrade")]


public class TutorialStep_GetUpgrade : TutorialStep
{
    [SerializeField] private UpgradeDataSO targetUpgrade;

    public override void HandleTask()
    {
        
    }

    public override void StartTask()
    {
        base.StartTask();
        UI_UpgradeSlot.OnUpgradeBuy += HandleTask;
        UpdateCurrentGoalUI();
    }

    private void HandleTask(UpgradeDataSO purchasedUpgrade)
    {
        if (purchasedUpgrade = targetUpgrade)
            Complete();
    }

    protected override void Complete()
    {
        base.Complete();
        DialogueManager.instance.SetPriorityDialogue(null);
    }

    public override void StopTask()
    {
        UI_UpgradeSlot.OnUpgradeBuy -= HandleTask;
    }

    public override void UpdateCurrentGoalUI()
    {
        string text = Localization.GetString("tutorial_step_get_upgrade") + ": " + $"<b>{targetUpgrade.GetUpgradeName()}</b>";
        UI.instance.inGameUI.UpdateCurrentGoal(text);
    }
}
