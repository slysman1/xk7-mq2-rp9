#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ItemDataTransferTool
{
    [MenuItem("Tools/Transfer Item Info To Item Data")]
    public static void TransferAll()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemDataSO");
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemDataSO data = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);

            if (data == null || data.itemPrefab == null)
                continue;

            Item_Base itemBase = data.itemPrefab.GetComponentInChildren<Item_Base>(true);

            if (itemBase == null)
                continue;

            //data.maxStackInHand = itemBase.GetMaxStack();
            //data.itemStackYoffset = itemBase.GetStackYOffset();
            //data.inHandPosition = itemBase.GetInHandPosition();
            //data.inHandRotation = itemBase.GetInHandRotation();
            //data.canStackWith = itemBase.GetCanStackWith();
            //data.kinematicOnImpact = itemBase.GetKinematicOnImpact();
            //data.weightType = itemBase.GetItemWeightType();
            //data.pickupType = itemBase.GetPickupTime() > 0 ? PickupType.Hold : PickupType.Click;

            EditorUtility.SetDirty(data);
            count++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Transferred data for {count} items.");
    }
}
#endif