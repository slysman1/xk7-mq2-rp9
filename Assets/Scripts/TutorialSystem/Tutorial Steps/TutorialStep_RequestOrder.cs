using UnityEngine;


[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Request Order", fileName = "Tutorial Step - Request Order")]


public class TutorialStep_RequestOrder : TutorialStep
{
    [SerializeField] protected OrderDataSO orderToStartNext;

    public override void HandleTask()
    {
        Complete();
    }

    public override void StartTask()
    {
        base.StartTask();


        if (orderToStartNext != null)
            TutorialManager.instance.SetTutorialOrder(orderToStartNext);

        TutorialIndicator.HighlightAllTargets<Order_RequestButton>();
        Order_RequestButton.OnOrderRequested += HandleTask;
        UpdateCurrentGoalUI();
    }

    public override void StopTask()
    {
        Order_RequestButton.OnOrderRequested -= HandleTask;
    }

    public override void UpdateCurrentGoalUI()
    {

        string text = Localization.GetString("tutorial_step_request_order");
        UI.instance.inGameUI.UpdateCurrentGoal(text);
    }
}
