using UnityEngine;
using System;

public abstract class TutorialStep : ScriptableObject
{

    public ItemDataSO[] taskReward;
    public ItemDataSO[] startingItems;

    [SerializeField] protected DialogueNodeSO dialogueToStartNext;
    [SerializeField] protected OrderDataSO orderToStartNext;


    public virtual void StartTask()
    {
        TutorialIndicator.Clear();
        
        if(orderToStartNext != null)
            TutorialManager.instance.SetTutorialOrder(orderToStartNext);

        if(dialogueToStartNext != null)
            DialogueManager.instance.SetPriorityDialogue(dialogueToStartNext);
    }

    public abstract void StopTask();

    public abstract void HandleTask();

    protected virtual void Complete()
    {
        StopTask();
        TutorialManager.instance.OnTaskCompleted(this);
    }

    public abstract void UpdateCurrentGoalUI();

}
