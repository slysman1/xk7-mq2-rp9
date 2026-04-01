#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class ItemDataAutoLinker
{
    [MenuItem("Tools/Auto-Assign ItemData to Prefabs")]
    public static void AutoAssignItemData()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemDataSO", new[] { "Assets/Data" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemDataSO data = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);

            if (data == null || data.itemPrefab == null)
                continue;

            // open prefab for editing
            GameObject prefab = data.itemPrefab;
            Item_Base itemBase = prefab.GetComponentInChildren<Item_Base>(true);

            if (itemBase == null)
            {
                Debug.LogWarning($"❌ No Item_Base found in prefab {prefab.name}");
                continue;
            }

            // Assign the SO to prefab
            itemBase.AssignItemData(data);// = data;

            // mark dirty so Unity saves it
            EditorUtility.SetDirty(itemBase);
            EditorUtility.SetDirty(prefab);
            EditorUtility.SetDirty(data);

            Debug.Log($"✅ Linked {data.name} <-> {prefab.name}");
        }

        AssetDatabase.SaveAssets();
    }
}
#endif
