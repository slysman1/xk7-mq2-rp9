using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Tool : Item_Base
{
    [Header("Tool Rules")]
    [SerializeField] protected List<ItemDataSO> allowedInteraction = new();
    protected readonly List<string> allowedItemIds = new();


    [Header("Interaction rules")]
    [SerializeField] protected float interactionDur  = 1;
    [Range(-1, 1)]
    [SerializeField] protected float arcMovement = .4f;
    [Range(0, 1)]
    [SerializeField] protected float arcMoveDur = .2f;
    protected Coroutine interactionCo;

    

    public override void Highlight(bool enable)
    {
        base.Highlight(enable);
        ShowInputUI(enable);
    }

    public virtual void PerformInteraction(Item_Base itemToInteractWith)
    {
        if (interactionCo == null)
            interactionCo = StartCoroutine(PerformInteractionCo(itemToInteractWith));
    }

    protected virtual IEnumerator PerformInteractionCo(Item_Base item)
    {
        yield return null;
    }



    public bool CanInteractWith(ItemDataSO itemData)
    {
        return allowedInteraction.Contains(itemData);
    }

    public void Upgrade(float speedMultiplier)
    {
        interactionDur = interactionDur / (1f + speedMultiplier);
    }
}
