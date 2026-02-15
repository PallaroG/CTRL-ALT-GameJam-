using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SuperAnimatedDialogue.Editor
{
    [CustomEditor(typeof(SuperAnimatedDialogue.Runtime.DialogueSystem))]
    public class DialogueSystemEditor : UnityEditor.Editor
    {
        GUIStyle headerStyle;
        GUIStyle foldoutStyle;
        GUIStyle nodeStyle;

        SerializedProperty startNode;
        SerializedProperty nodes;

        // UI
        SerializedProperty panel;
        SerializedProperty dialogueText;
        SerializedProperty choicesParent;
        SerializedProperty choiceButtonPrefab;

        bool showUISettings = true;
        bool showGeneralSettings = true;
        bool showNodes = true;

        void OnEnable()
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };

            foldoutStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };

            nodeStyle = new GUIStyle("box")
            {
                padding = new RectOffset(10, 10, 10, 10)
            };

            // General
            startNode = serializedObject.FindProperty("startNode");
            nodes = serializedObject.FindProperty("nodes");

            // UI
            panel = serializedObject.FindProperty("panel");
            dialogueText = serializedObject.FindProperty("dialogueText");
            choicesParent = serializedObject.FindProperty("choicesParent");
            choiceButtonPrefab =
                serializedObject.FindProperty("choiceButtonPrefab");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader("DIALOGUE SYSTEM");
            EditorGUILayout.Space();

            DrawSection("UI Elements", ref showUISettings, () =>
            {
                EditorGUILayout.PropertyField(panel);
                EditorGUILayout.PropertyField(dialogueText);
                EditorGUILayout.PropertyField(choicesParent);
                EditorGUILayout.PropertyField(choiceButtonPrefab);

                if (choicesParent.objectReferenceValue == null ||
                    choiceButtonPrefab.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox(
                        "For choices to work, both 'Choices Parent' and 'Choice Button Prefab' must be assigned. Otherwise, the node will be treated as a node without choices.",
                        MessageType.Warning
                    );
                }
            });

            EditorGUILayout.Space();

            DrawSection("General Settings", ref showGeneralSettings, () =>
            {
                EditorGUILayout.PropertyField(startNode);
            });

            EditorGUILayout.Space();

            DrawSection("Dialogue Nodes", ref showNodes, () =>
            {
                EditorGUILayout.PropertyField(
                    nodes.FindPropertyRelative("Array.size")
                );

                if (showNodes)
                {
                    for (int i = 0; i < nodes.arraySize; i++)
                    {
                        DrawNode(nodes.GetArrayElementAtIndex(i), i);
                    }
                }
            });

            serializedObject.ApplyModifiedProperties();
        }

        void DrawNode(SerializedProperty node, int index)
        {
            EditorGUILayout.BeginVertical(nodeStyle);

            SerializedProperty text = node.FindPropertyRelative("text");
            SerializedProperty autoNextNode =
                node.FindPropertyRelative("autoNextNode");
            SerializedProperty choices = node.FindPropertyRelative("choices");

            EditorGUILayout.LabelField(
                $"Node {index}",
                EditorStyles.boldLabel
            );

            EditorGUILayout.PropertyField(text);

            if (choices.arraySize == 0)
            {
                EditorGUILayout.PropertyField(autoNextNode);
                EditorGUILayout.HelpBox(
                    "This node has no choices. It will advance when the player presses confirm (keyboard or controller). Use -1 to end the dialogue.",
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "This node has choices. 'Auto Next Node' will be ignored.",
                    MessageType.None
                );
            }

            EditorGUILayout.PropertyField(choices, true);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        void DrawHeader(string title)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 34);
            EditorGUI.DrawRect(
                rect,
                new Color(0.07f, 0.36f, 0.5f, 1f)
            );
            GUI.Label(rect, title, headerStyle);
        }

        void DrawSection(
            string title,
            ref bool foldout,
            System.Action content
        )
        {
            EditorGUILayout.BeginVertical("box");
            foldout = EditorGUILayout.Foldout(
                foldout,
                title,
                true,
                foldoutStyle
            );

            if (foldout)
            {
                EditorGUILayout.Space(4);
                content.Invoke();
            }

            EditorGUILayout.EndVertical();
        }
    }
}
