using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;
    private DeliveryManager deliveryManager => DeliveryManager.instance;
    private HashSet<TutorialStep> completedTutorialSteps = new HashSet<TutorialStep>();

    public TutorialStep currentStep;
    [SerializeField] private TutorialSequenceSO tutorial;
    private int stepIndex;
    public int startingQuest;

    [Space]
    [SerializeField] private TutorialStep stepToCompleteToTakeOrders;
    [SerializeField] private OrderDataSO tutorialOrder;





    private void Awake()
    {
        instance = this;
    }

    private IEnumerator Start()
    {
        stepIndex = startingQuest;
        yield return null;
        StartNextTutorialStep();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            OnTaskCompleted(currentStep);
    }

    private void StartNextTutorialStep()
    {
        if (stepIndex >= tutorial.steps.Length)
        {
            //   UI.instance.inGameUI.UpdateCurrentGoal("");
            return;
        }

        StartCoroutine(StartNextTutorialStepCo());
    }

    private IEnumerator StartNextTutorialStepCo()
    {

        yield return new WaitForSeconds(.5f);

        currentStep = tutorial.steps[stepIndex];
        stepIndex = stepIndex + 1;

        currentStep.OnCompleted += OnTaskCompleted;
        CreateItems(currentStep.startingItems);

        yield return null;
        currentStep.StartTask();
    }

    private void OnTaskCompleted(TutorialStep tutorialStep)
    {
        completedTutorialSteps.Add(tutorialStep);

        tutorialStep.OnCompleted -= OnTaskCompleted;

        CreateItems(tutorialStep.taskReward);
        StartNextTutorialStep();
    }

    private void CreateItems(ItemDataSO[] items)
    {
        if (items == null || items.Length == 0)
            return;

        if (items.Length > 0 && items[0] == null)
            return;

        deliveryManager.CreateDeliveryBox(items.ToList());
    }

    public bool CompletedStepNeededToTakeOrders()
    {
        if (stepToCompleteToTakeOrders == null)
            return true;

        return completedTutorialSteps.Contains(stepToCompleteToTakeOrders);
    }

    public void SetTutorialOrder(OrderDataSO orderData) => tutorialOrder = orderData;
    public OrderDataSO GetTutorialOrder() => tutorialOrder;


}
