using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class HierarchyColor
{
    static Color backgroundColor = new Color(0.14f, 0.14f, 0.14f, 1f); // cinza escuro


    static HierarchyColor()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj == null) return;

        if (!obj.name.StartsWith("#")) return;

        if (Event.current.type == EventType.Repaint)
        {
            EditorGUI.DrawRect(selectionRect, backgroundColor);

            // cria estilo baseado no padrão da hierarchy
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.normal.textColor = Color.white;

            // remove o "#" do nome exibido
            string cleanName = obj.name.Substring(1);

            // ajusta posição para não cobrir o ícone
            Rect labelRect = selectionRect;
            labelRect.x += 16;

            EditorGUI.LabelField(labelRect, cleanName, style);
        }
    }
}