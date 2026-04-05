using UnityEngine;
using System;

public abstract class TutorialStep : ScriptableObject
{

    public ItemDataSO[] taskReward;
    public ItemDataSO[] startingItems;

    public Action<TutorialStep> OnCompleted; // notify system
    [SerializeField] protected DialogueNodeSO dialogueToStartNext;


    public virtual void StartTask()
    {
        TutorialIndicator.Clear();


        if(dialogueToStartNext != null)
            DialogueManager.instance.SetPriorityDialogue(dialogueToStartNext);
    }

    public abstract void StopTask();

    public abstract void HandleTask();

    protected virtual void Complete()
    {
        OnCompleted?.Invoke(this);
        StopTask();
    }

    public abstract void UpdateCurrentGoalUI();

}
