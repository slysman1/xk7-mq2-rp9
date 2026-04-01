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

    public TutorialStep currentTask;
    [SerializeField] private TutorialStep[] tutorialTasks;
    private int currentTaskIndex;
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
        currentTaskIndex = startingQuest;
        yield return null;
        StartNextTutorialStep();
        //InvokeRepeating(nameof(UpdateCurrentTaskGoalText), 0, .1f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            OnTaskCompleted(currentTask);

        //currentTask?.Update();
    }

    private void StartNextTutorialStep()
    {
        if (currentTaskIndex >= tutorialTasks.Length)
        {
            UI.instance.inGameUI.UpdateCurrentGoal("");
            return;
        }

        StartCoroutine(StartNextTutorialStepCo());
    }

    private IEnumerator StartNextTutorialStepCo()
    {

        yield return new WaitForSeconds(.5f);

        currentTask = tutorialTasks[currentTaskIndex];
        currentTaskIndex = currentTaskIndex + 1;

        currentTask.OnCompleted += OnTaskCompleted;
        CreateItems(currentTask.startingItems);

        yield return null;
        currentTask.StartTask();
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
        if (currentTask == fallbackStep)
            return;

        // Save current
        if (currentTask != null)
        {
            currentTask.OnCompleted -= OnTaskCompleted;
            currentTask.StopTask();

            previousStep = currentTask;
        }

        isInFallback = true;

        // Start fallback
        currentTask = fallbackStep;

        currentTask.OnCompleted += OnFallbackCompleted;
        CreateItems(currentTask.startingItems);
        currentTask.StartTask();
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
            currentTask = previousStep;
            previousStep = null;

            currentTask.OnCompleted += OnTaskCompleted;
            currentTask.StartTask();
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
