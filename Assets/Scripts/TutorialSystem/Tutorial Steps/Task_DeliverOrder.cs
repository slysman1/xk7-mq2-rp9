using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Next Task Data/Task data - Deliver job", fileName = "Next task data - 08 -")]

public class Task_DeliverOrder : TutorialStep
{
    [SerializeField] private bool highlightDeliveryDoor;

    public override void HandleTask()
    {
        Complete();
    }

    public override void StartTask()
    {
        base.StartTask();
        OrderManager.OnOrderCompleted += HandleTask;
        TutorialIndicator.HighlightTarget<Order_DeliveryManager>();

        //if (highlightDeliveryDoor)
        //    FindFirstObjectByType<Quest_DeliveryButton>().Highlight(true);


        UpdateCurrentGoalUI();
    }

    public override void StopTask()
    {
        //FindFirstObjectByType<Quest_DeliveryButton>().Highlight(false);
        OrderManager.OnOrderCompleted -= HandleTask;
    }

    public override void UpdateCurrentGoalUI()
    {
        string text = Localization.GetString("goal_complete_order");
        UI.instance.inGameUI.UpdateCurrentGoal(text);
    }
}
