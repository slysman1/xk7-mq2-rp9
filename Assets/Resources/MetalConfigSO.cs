using UnityEngine;

[CreateAssetMenu(fileName = "MetalConfig", menuName = "Config/MetalConfig")]

public class MetalConfigSO : ScriptableObject
{
    public ItemDataSO[] copperItems;
    public ItemDataSO[] silverItems;
    public ItemDataSO[] goldItems;

    [Space]
    public ItemDataSO[] allCoinsData;
}