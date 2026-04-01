using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Buy Upgrade", fileName = "Tutorial Step - Buy Upgrade")]
public class TutorialStep_GetUpgrade : TutorialStep
{
    [SerializeField] private UpgradeDataSO targetUpgrade;
    private System.Action<UpgradeDataSO> onUpgradeBuy;

    public override void HandleTask() { }

    public override void StartTask()
    {
        base.StartTask();
        onUpgradeBuy = upgrade => { if (upgrade == targetUpgrade) Complete(); };
        UI_UpgradeSlot.OnUpgradeBuy += onUpgradeBuy;
        UpdateCurrentGoalUI();
    }

    public override void StopTask()
    {
        UI_UpgradeSlot.OnUpgradeBuy -= onUpgradeBuy;
    }

    public override void UpdateCurrentGoalUI()
    {
        string text = Localization.GetString("tutorial_step_get_upgrade") + ": " + $"<b>{targetUpgrade.GetUpgradeName()}</b>";
        UI.instance.inGameUI.UpdateCurrentGoal(text);
    }
}