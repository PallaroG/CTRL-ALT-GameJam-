using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using SuperAnimatedDialogue.Runtime;

namespace SuperAnimatedDialogue.Editor
{
    [CustomEditor(typeof(TextAnimationController))]
    public class TextAnimationControllerEditor : UnityEditor.Editor
    {

        private GUIStyle headerStyle;
        private GUIStyle foldoutStyle;
        private GUIStyle previewStyle;

        private static readonly Color ActiveColor   = new Color(0.50f, 0.25f, 0.60f, 0.18f);
        private static readonly Color InactiveColor = new Color(0, 0, 0, 0);

        private SerializedProperty profiles;
        private SerializedProperty audioSource;
        private SerializedProperty charSound;
        private SerializedProperty pitchVariation;

        private bool showAudio = true;
        private Dictionary<string, bool> profileFoldoutStates = new Dictionary<string, bool>();

        // Preview State
        private string previewText = "Preview Text";
        private bool isLivePreview = false;
        private float previewTime = 0f;
        private int selectedProfileIndex = 0;

        private void OnEnable()
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

            previewStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 24,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            profiles       = serializedObject.FindProperty("profiles");
            audioSource    = serializedObject.FindProperty("audioSource");
            charSound      = serializedObject.FindProperty("charSound");
            pitchVariation = serializedObject.FindProperty("charSoundPitchVariation");

            EditorApplication.update += EditorUpdate;
        }

        private void OnDisable()
        {

            EditorApplication.update -= EditorUpdate;
        }

        private void EditorUpdate()
        {
            if (!isLivePreview) return;

            previewTime += Time.deltaTime;
            Repaint();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader("TEXT ANIMATION CONTROLLER");
            EditorGUILayout.Space();

            DrawGlobalPreviewSettings();
            EditorGUILayout.Space();

            DrawProfiles();
            EditorGUILayout.Space();

            DrawSection("Audio", ref showAudio, () =>
            {
                EditorGUILayout.PropertyField(audioSource);
                EditorGUILayout.PropertyField(charSound);
                EditorGUILayout.PropertyField(pitchVariation);
            });

            serializedObject.ApplyModifiedProperties();
        }


        private void DrawGlobalPreviewSettings()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Live Preview Settings", EditorStyles.boldLabel);

            previewText = EditorGUILayout.TextField("Preview Text", previewText);

            bool newLivePreview = EditorGUILayout.Toggle("Live Preview", isLivePreview);
            if (newLivePreview != isLivePreview)
            {
                isLivePreview = newLivePreview;
                if (isLivePreview)
                    previewTime = 0f;
            }

            if (!isLivePreview)
                previewTime = EditorGUILayout.Slider("Preview Time", previewTime, 0f, 5f);

            EditorGUILayout.EndVertical();
        }

        private void DrawProfiles()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Animation Profiles", EditorStyles.boldLabel);

            for (int i = 0; i < profiles.arraySize; i++)
            {
                SerializedProperty profile = profiles.GetArrayElementAtIndex(i);
                SerializedProperty profileName = profile.FindPropertyRelative("profileName");

                if (i == 0)
                    profileName.stringValue = "Default";

                string key = profileName.stringValue + i;
                if (!profileFoldoutStates.ContainsKey(key))
                    profileFoldoutStates[key] = false;

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                profileFoldoutStates[key] =
                    EditorGUILayout.Foldout(profileFoldoutStates[key], profileName.stringValue, true, foldoutStyle);

                if (i > 0)
                    profileName.stringValue = EditorGUILayout.TextField(profileName.stringValue);
                else
                    EditorGUILayout.LabelField(profileName.stringValue, EditorStyles.boldLabel);

                if (i > 0 && GUILayout.Button("X", GUILayout.Width(20)))
                {
                    profiles.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    return;
                }

                EditorGUILayout.EndHorizontal();

                if (profileFoldoutStates[key])
                {
                    EditorGUILayout.Space(4);

                    if (GUILayout.Button($"Preview This Profile: {profileName.stringValue}"))
                    {
                        selectedProfileIndex = i;
                        previewTime = 0f;
                    }

                    if (selectedProfileIndex == i)
                        DrawProfilePreview(profile);

                    DrawEffect(profile, "Typewriter", "typewriter", prop =>
                    {
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("charsPerSecond"));
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("randomDelay"));

                        if (prop.FindPropertyRelative("randomDelay").boolValue)
                            EditorGUILayout.PropertyField(prop.FindPropertyRelative("delayRange"));
                    });

                    DrawEffect(profile, "Wave", "wave", prop => DrawProps(prop, "amplitude", "frequency", "speed"));
                    DrawEffect(profile, "Shake", "shake", prop => DrawProps(prop, "magnitude", "speed"));
                    DrawEffect(profile, "Bounce", "bounce", prop => DrawProps(prop, "height", "speed"));
                    DrawEffect(profile, "Scale", "scale", prop => DrawProps(prop, "scaleMultiplier", "speed"));
                    DrawEffect(profile, "Rotate", "rotate", prop => DrawProps(prop, "angle", "speed"));
                    DrawEffect(profile, "Jitter", "jitter", prop => DrawProps(prop, "magnitude", "speed"));

                    DrawEffect(profile, "Impact", "impact", prop =>
                    {
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("loop"));
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("impactDistance"));
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("impactDuration"));
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("impactCurve"));
                    });

                    DrawEffect(profile, "Perspective", "perspective", prop =>
                    {
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("depthCurve"));
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("depthScale"));
                    });

                    DrawEffect(profile, "Rainbow", "rainbow",
                        prop => DrawProps(prop, "speed", "saturation", "brightness", "offset"));
                }

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add New Profile"))
            {
                profiles.arraySize++;
                profiles.GetArrayElementAtIndex(profiles.arraySize - 1)
                    .FindPropertyRelative("profileName").stringValue = $"New Profile {profiles.arraySize - 1}";
            }

            EditorGUILayout.EndVertical();
        }


        private void DrawProfilePreview(SerializedProperty profile)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("box");

            Rect previewRect = EditorGUILayout.GetControlRect(false, 50);
            EditorGUI.DrawRect(previewRect, new Color(0.1f, 0.1f, 0.1f, 1f));

            Vector2 center = previewRect.center;
            float totalWidth = previewStyle.CalcSize(new GUIContent(previewText)).x;
            float currentX = center.x - totalWidth / 2f;

            for (int i = 0; i < previewText.Length; i++)
            {
                string c = previewText[i].ToString();
                Vector2 size = previewStyle.CalcSize(new GUIContent(c));

                Vector3 offset = CalculateCharacterOffset(profile, i, previewTime);
                Quaternion rotation = CalculateCharacterRotation(profile, i, previewTime);
                float scale = CalculateCharacterScale(profile, i, previewTime);

                Rect charRect = new Rect(currentX + offset.x, center.y - size.y / 2f + offset.y, size.x, size.y);

                Matrix4x4 matrix = GUI.matrix;
                Color originalColor = GUI.color;

                SerializedProperty rainbow = profile.FindPropertyRelative("rainbow");
                if (rainbow.FindPropertyRelative("enabled").boolValue)
                {
                    float hue = (previewTime * rainbow.FindPropertyRelative("speed").floatValue +
                                 i * rainbow.FindPropertyRelative("offset").floatValue) % 1f;
                    GUI.color = Color.HSVToRGB(
                        hue,
                        rainbow.FindPropertyRelative("saturation").floatValue,
                        rainbow.FindPropertyRelative("brightness").floatValue);
                }

                Vector2 pivot = charRect.center;
                GUIUtility.ScaleAroundPivot(Vector2.one * scale, pivot);
                GUIUtility.RotateAroundPivot(rotation.eulerAngles.z, pivot);
                GUI.Label(charRect, c, previewStyle);

                GUI.matrix = matrix;
                GUI.color = originalColor;

                currentX += size.x;
            }

            EditorGUILayout.EndVertical();
        }


        private Vector3 CalculateCharacterOffset(SerializedProperty profile, int index, float time)
        {
            Vector3 offset = Vector3.zero;

            SerializedProperty wave = profile.FindPropertyRelative("wave");
            if (wave.FindPropertyRelative("enabled").boolValue)
            {
                offset.y += Mathf.Sin(time * wave.FindPropertyRelative("frequency").floatValue + index *
                                      wave.FindPropertyRelative("speed").floatValue) *
                            wave.FindPropertyRelative("amplitude").floatValue;
            }

            SerializedProperty shake = profile.FindPropertyRelative("shake");
            if (shake.FindPropertyRelative("enabled").boolValue)
            {
                float mag = shake.FindPropertyRelative("magnitude").floatValue;
                offset.x += (Mathf.PerlinNoise(time * 10f, index) - 0.5f) * mag;
                offset.y += (Mathf.PerlinNoise(time * 10f + 10f, index) - 0.5f) * mag;
            }

            SerializedProperty bounce = profile.FindPropertyRelative("bounce");
            if (bounce.FindPropertyRelative("enabled").boolValue)
                offset.y += Mathf.Abs(Mathf.Sin(time * bounce.FindPropertyRelative("speed").floatValue + index * 0.1f))
                            * bounce.FindPropertyRelative("height").floatValue;

            SerializedProperty jitter = profile.FindPropertyRelative("jitter");
            if (jitter.FindPropertyRelative("enabled").boolValue)
                offset.x += Mathf.Sin(time * jitter.FindPropertyRelative("speed").floatValue + index) *
                            jitter.FindPropertyRelative("magnitude").floatValue;

            return offset;
        }

        private Quaternion CalculateCharacterRotation(SerializedProperty profile, int index, float time)
        {
            SerializedProperty rotate = profile.FindPropertyRelative("rotate");
            if (!rotate.FindPropertyRelative("enabled").boolValue) return Quaternion.identity;

            float angle = Mathf.Sin(time * rotate.FindPropertyRelative("speed").floatValue + index) *
                          rotate.FindPropertyRelative("angle").floatValue;
            return Quaternion.Euler(0, 0, angle);
        }

        private float CalculateCharacterScale(SerializedProperty profile, int index, float time)
        {
            SerializedProperty scale = profile.FindPropertyRelative("scale");
            if (!scale.FindPropertyRelative("enabled").boolValue) return 1f;

            return 1f + Mathf.Sin(time * scale.FindPropertyRelative("speed").floatValue + index) *
                   scale.FindPropertyRelative("scaleMultiplier").floatValue;
        }


        private void DrawHeader(string title)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 34);
            EditorGUI.DrawRect(rect, new Color(0.36f, 0.07f, 0.5f, 1f));
            GUI.Label(rect, title, headerStyle);
        }

        private void DrawSection(string title, ref bool foldout, System.Action content)
        {
            EditorGUILayout.BeginVertical("box");
            foldout = EditorGUILayout.Foldout(foldout, title, true, foldoutStyle);

            if (foldout)
            {
                EditorGUILayout.Space(4);
                content.Invoke();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawEffect(SerializedProperty root, string title, string propName,
            System.Action<SerializedProperty> content)
        {
            SerializedProperty effect = root.FindPropertyRelative(propName);
            SerializedProperty enabled = effect.FindPropertyRelative("enabled");

            Rect bg = EditorGUILayout.BeginVertical("box");
            EditorGUI.DrawRect(bg, enabled.boolValue ? ActiveColor : InactiveColor);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(enabled, new GUIContent("Enabled"));
            EditorGUILayout.EndHorizontal();

            if (enabled.boolValue)
            {
                EditorGUILayout.Space(4);
                content.Invoke(effect);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawProps(SerializedProperty root, params string[] names)
        {
            foreach (string n in names)
                EditorGUILayout.PropertyField(root.FindPropertyRelative(n));
        }
    }
}
