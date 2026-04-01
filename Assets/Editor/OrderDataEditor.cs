using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OrderDataSO))]
[CanEditMultipleObjects]
public class OrderDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Order"))
        {
            foreach (var t in targets)
            {
                OrderDataSO order = (OrderDataSO)t;

                Undo.RecordObject(order, "Generate Order");

                OrderGenerator.Generate(order);

                EditorUtility.SetDirty(order);
            }

            AssetDatabase.SaveAssets();
        }
    }
}