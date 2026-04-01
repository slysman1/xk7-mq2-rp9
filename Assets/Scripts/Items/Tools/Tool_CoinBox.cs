using System.Collections;
using UnityEngine;
using static Alexdev.TweenUtils;

public class Tool_CoinBox : Item_Tool
{
    //private Holder_CoinBox itemHolder;

    //protected override void Awake()
    //{
    //    base.Awake();
    //    itemHolder = GetComponent<Holder_CoinBox>();
    //}

    //public override void PerformInteraction(Item_Base itemToInteractWith, Player player)
    //{
    //    if (itemHolder.ItemCanBePlaced(itemToInteractWith) == false)
    //        return;

    //    base.PerformInteraction(itemToInteractWith, player);
    //}

    //protected override IEnumerator PerformInteractionCo(Item_Base item)
    //{
    //    yield return null;
    //    item.EnableKinematic(true);
    //    Vector3 target = transform.position;
    //    Vector3 offset = Vector3.zero;// Vector3.up * .2f;

    //    //yield return StartCoroutine(ArcMovement(item.transform, transform, offset, arcMovement, interactionDuration));

    //    itemHolder.AddDirectlyToHolder(item);
    //    interactionCo = null;
    //}
}
