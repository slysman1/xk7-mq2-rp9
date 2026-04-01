using UnityEngine;

public class Item_CollectableCoin : Item_Base
{
    [SerializeField] private CollectableCoinType collectableType;


    public CollectableCoinType GetCollectableType() => collectableType;

}
