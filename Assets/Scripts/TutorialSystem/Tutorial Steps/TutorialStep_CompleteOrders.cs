using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Complete Orders", fileName = "Tutorial Step - Complete Orders")]


public class TutorialStep_CompleteOrders : TutorialStep
{
    [SerializeField] private int ordersToComplete = 1;
    private int completedOrders;
    [SerializeField] private float waitTillHelpIndicator = 15f;
    private Coroutine helpCo;

    public override void StartTask()
    {
        base.StartTask();

        completedOrders = 0;
        OrderManager.OnOrderCompleted += HandleTask;
        Order_RequestButton.OnOrderRequested += TutorialIndicator.Clear;

        if (waitTillHelpIndicator > 0)
            helpCo = TutorialManager.instance.StartCoroutine(EnableHelpIndicatorIfNeededCo());


        UpdateCurrentGoalUI();
    }

    public override void HandleTask()
    {
        completedOrders++;
        UpdateCurrentGoalUI();

        if (completedOrders >= ordersToComplete)
            Complete();
    }


    public override void StopTask()
    {
        if(helpCo != null)
            TutorialManager.instance.StopCoroutine(helpCo);

        OrderManager.OnOrderCompleted -= HandleTask;    
        Order_RequestButton.OnOrderRequested -= TutorialIndicator.Clear;
    }

    public override void UpdateCurrentGoalUI()
    {
        string text = $"{Localization.GetString("tutorial_step_complete_orders")}: {completedOrders}/{ordersToComplete}";
        UI.instance.inGameUI.UpdateCurrentGoal(text);
    }

    private IEnumerator EnableHelpIndicatorIfNeededCo()
    {
        if (OrderManager.instance.remainingOrders.Count > 0 || OrderManager.instance.trackedOrders.Count > 0)
            yield break;

        yield return new WaitForSeconds(waitTillHelpIndicator);
        TutorialIndicator.HighlightAllTargets<Order_RequestButton>();
    }

}
