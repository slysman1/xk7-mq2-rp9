using UnityEngine;

[CreateAssetMenu(menuName = "Orders/Order Input Item Data")]

public class OrderInputItemDataSO : ScriptableObject
{
    public ItemDataSO input;
    public ItemDataSO result;
    public int coinQuantity;
}
