using UnityEngine;

public class CollectableStandHolder_CoinsSlot : MonoBehaviour
{
    [SerializeField] private CollectableCoinType collectableType;

    public CollectableCoinType GetCollectableType() => collectableType;
}

