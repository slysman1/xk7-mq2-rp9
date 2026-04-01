using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Open Delivery Box", fileName = "Tutorial Step - Open Delivery Box")]


public class TutorialStep_OpenDeliveryBox : TutorialStep
{
    
    public override void HandleTask()
    {
        Complete();
    }


    public override void StartTask()
    {
        base.StartTask();


        Item_DeliveryBox deliveryBox = FindFirstObjectByType<Item_DeliveryBox>();

        if (deliveryBox == null)
        {
            Debug.Log("No box found > skip to next tutorial step");
            Complete();
            return;
        }

        UpdateCurrentGoalUI();

        Item_DeliveryBox.OnBoxOpened += HandleTask;
        TutorialIndicator.HighlightTarget<Item_DeliveryBox>();


    }

    public override void StopTask()
    {
        Item_DeliveryBox.OnBoxOpened -= HandleTask;
    }

    public override void UpdateCurrentGoalUI()
    {
        string goalText = Localization.GetString("tutorial_step_open_box");
        UI.instance.inGameUI.UpdateCurrentGoal(goalText);
    }
}
