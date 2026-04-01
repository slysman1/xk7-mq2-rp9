#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


[CreateAssetMenu(menuName = "Item Data/Item Data", fileName = "Item data - ")]
public class ItemDataSO : ScriptableObject
{
    public string itemId;
    public GameObject itemPrefab;
    public string emojiIcon;
    public int creditValue;

    [Header("Item Details")]
    public KinematicOnImpact kinematicOnImpact;
    public ItemWeightType weightType;
    public PickupType pickupType;


    [Header("Placement details")]
    public PlacementType placementType;
    public float placementForwardOffset = 0f;

    [Header("Stack Details")]
    public int maxStackInHand = 1;
    public float itemStackYoffset = 0.1f;
    public ItemDataSO[] canStackWith;

    [Header("In Hand Carry")]
    public Vector3 inHandPosition;
    public Vector3 inHandRotation;

    private void OnValidate()
    {
#if UNITY_EDITOR

        
        if (itemPrefab == null)
        {
            Debug.Log("Has no prefab assigned: " + name, this);
            return;
        }

        if (string.IsNullOrEmpty(itemId))
            itemId = itemPrefab.name + " || " + System.Guid.NewGuid().ToString();

        //if (itemPrice == 0 && category != ItemCategoryType.None)
        //    itemPrice = Random.Range(1, 5);
        

        if (itemPrefab != null)
        {
            Item_Base itemBase = itemPrefab.GetComponentInChildren<Item_Base>(true);

            if (itemBase != null)
            {
                // Always overwrite to be safe
                itemBase.AssignItemData(this);

                // Make sure Unity knows the prefab was modified
                EditorUtility.SetDirty(itemBase);
                EditorUtility.SetDirty(itemPrefab);
            }
        }
#endif
    }

    [ContextMenu("Update item data on prefab")]
    public void UpdateItemDataOnPrefab()
    {
        if (itemPrefab != null)
        {
            Item_Base itemBase = itemPrefab.GetComponentInChildren<Item_Base>(true);

            if (itemBase != null)
                itemBase.AssignItemData(this);
            // Always overwrite to be safe
        }
    }
}
