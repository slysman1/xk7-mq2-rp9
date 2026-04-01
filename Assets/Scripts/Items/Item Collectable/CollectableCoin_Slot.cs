using UnityEngine;

public class CollectableCoin_Slot : MonoBehaviour
{
    [SerializeField] private CollectableCoinType collectableType;

    public CollectableCoinType GetCollectableType() => collectableType;
}

