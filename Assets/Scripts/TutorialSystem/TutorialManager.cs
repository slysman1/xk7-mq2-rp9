using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;
    public Tutorial_SilverStamp silverStampTutorial { get; private set; }
    private DeliveryManager deliveryManager => DeliveryManager.instance;
    private HashSet<TutorialStep> completedTutorialSteps = new HashSet<TutorialStep>();
    private TutorialStep previousStep;
    private bool isInFallback;

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
        silverStampTutorial = GetComponent<Tutorial_SilverStamp>();
    }

    private IEnumerator Start()
    {
        stepIndex = startingQuest;
        yield return null;
        StartNextTutorialStep();
        //InvokeRepeating(nameof(UpdateCurrentTaskGoalText), 0, .1f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            OnTaskCompleted(currentStep);

        //currentTask?.Update();
    }

    private void StartNextTutorialStep()
    {
        if (stepIndex >= tutorial.steps.Length)
        {
            UI.instance.inGameUI.UpdateCurrentGoal("");
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
        if(items == null || items.Length == 0)
            return;

        if (items.Length > 0 && items[0] == null)
            return;

        deliveryManager.SendItems(items);
    }

    public bool CompletedStepNeededToTakeOrders()
    {
        if (stepToCompleteToTakeOrders == null)
            return true;

        return completedTutorialSteps.Contains(stepToCompleteToTakeOrders);
    }

    public void SetTutorialOrder(OrderDataSO orderData) => tutorialOrder = orderData;
    public OrderDataSO GetTutorialOrder(out OrderDataSO tutorialOrder)
    {
        tutorialOrder = this.tutorialOrder; // assign your stored quest
        return tutorialOrder;
    }

    public void ForceStartFallbackStep(TutorialStep fallbackStep)
    {
        if (fallbackStep == null)
        {
            ResetFallbackTutorial();
            return;
        }

        // Already in fallback → ignore new ones
        if (isInFallback)
            return;

        // Already running → ignore
        if (currentStep == fallbackStep)
            return;

        // Save current
        if (currentStep != null)
        {
            currentStep.OnCompleted -= OnTaskCompleted;
            currentStep.StopTask();

            previousStep = currentStep;
        }

        isInFallback = true;

        // Start fallback
        currentStep = fallbackStep;

        currentStep.OnCompleted += OnFallbackCompleted;
        CreateItems(currentStep.startingItems);
        currentStep.StartTask();
    }
    private void OnFallbackCompleted(TutorialStep step)
    {
        //completedTutorialSteps.Add(step);

        step.OnCompleted -= OnFallbackCompleted;
        //CreateItems(step.taskReward);

        isInFallback = false;

        // Return to previous
        if (previousStep != null)
        {
            currentStep = previousStep;
            previousStep = null;

            currentStep.OnCompleted += OnTaskCompleted;
            currentStep.StartTask();
        }
        else
        {
            StartNextTutorialStep();
        }
    }

    public void ResetFallbackTutorial()
    {
        if (!isInFallback)
            return;

        isInFallback = false;
        previousStep = null;
    }
}
