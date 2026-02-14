using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#region EFFECT SETTINGS (SIMPLE & ASSET FRIENDLY)

[System.Serializable]
public struct WaveSettings
{
    public bool enabled;
    public float amplitude;
    public float frequency;
    public float speed;
}

[System.Serializable]
public struct ShakeSettings
{
    public bool enabled;
    public float magnitude;
    public float speed;
}

[System.Serializable]
public struct BounceSettings
{
    public bool enabled;
    public float height;
    public float speed;
}

[System.Serializable]
public struct ScaleSettings
{
    public bool enabled;
    public float scaleMultiplier;
    public float speed;
}

[System.Serializable]
public struct RotateSettings
{
    public bool enabled;
    public float angle;
    public float speed;
}

[System.Serializable]
public struct JitterSettings
{
    public bool enabled;
    public float magnitude;
    public float speed;
}

[System.Serializable]
public struct ImpactSettings
{
    public bool enabled;
    public bool loop;
    public float impactDistance;
    public float impactDuration;
    public AnimationCurve impactCurve;
}

[System.Serializable]
public struct PerspectiveSettings
{
    public bool enabled;
    public AnimationCurve depthCurve;
    public float depthScale;
}

[System.Serializable]
public struct TypewriterSettings
{
    public bool enabled;
    public float charsPerSecond;
    public bool randomDelay;
    public Vector2 delayRange;
}

[System.Serializable]
public struct RainbowSettings
{
    public bool enabled;
    public float speed;
    public float saturation;
    public float brightness;
    public float offset;
}

[System.Serializable]
public class AnimationProfile
{
    public string profileName = "Default";

    [Header("Typewriter")]
    public TypewriterSettings typewriter;

    [Header("Effects")]
    public WaveSettings wave;
    public ShakeSettings shake;
    public BounceSettings bounce;
    public ScaleSettings scale;
    public RotateSettings rotate;
    public JitterSettings jitter;
    public ImpactSettings impact;
    public PerspectiveSettings perspective;
    public RainbowSettings rainbow;
}

#endregion

namespace SuperAnimatedDialogue.Runtime
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextAnimationController : MonoBehaviour
    {
        [Header("Animation Profiles")]
        public List<AnimationProfile> profiles = new List<AnimationProfile> { new AnimationProfile() };

        [Header("Audio")]
        public AudioSource audioSource;
        public AudioClip charSound;
        public float charSoundPitchVariation = 0.1f;

        TextMeshProUGUI tmpText;
        TMP_TextInfo textInfo;

        string originalText;
        float typeTimer;
        int visibleCharacterCount;
        float[] charImpactTimers;
        int[] characterProfileIndices;

        // Maps profile name to its index
        Dictionary<string, int> profileNameToIndex = new Dictionary<string, int>();

        void Awake()
        {
            tmpText = GetComponent<TextMeshProUGUI>();
            SetText(tmpText.text);
        }

        void Update()
        {
            HandleTypewriter();
            AnimateCharacters();
        }

        public void SetText(string newText)
        {

            profileNameToIndex.Clear();
            for (int i = 0; i < profiles.Count; i++)
            {
                profileNameToIndex[profiles[i].profileName] = i;
            }


            string textWithoutTags = ParseAnimationProfiles(newText);
            originalText = textWithoutTags;
            tmpText.text = textWithoutTags;

            typeTimer = 0f;
            visibleCharacterCount = 0;
            tmpText.maxVisibleCharacters = 0;

            tmpText.ForceMeshUpdate();


            int charCount = tmpText.textInfo.characterCount;
            charImpactTimers = new float[charCount];

            if (characterProfileIndices.Length != charCount)
            {

                characterProfileIndices = new int[charCount];
                for (int i = 0; i < charCount; i++)
                {
                    characterProfileIndices[i] = 0;
                }
            }
        }

        void HandleTypewriter()
        {
            if (profiles.Count == 0) return;

  
            TypewriterSettings typewriter = profiles[0].typewriter;


            bool anyTypewriterEnabled = false;
            foreach (var profile in profiles)
            {
                if (profile.typewriter.enabled)
                {
                    anyTypewriterEnabled = true;
                    typewriter = profile.typewriter; 
                    break;
                }
            }

            if (!anyTypewriterEnabled)
            {
                tmpText.maxVisibleCharacters = int.MaxValue;
                return;
            }

            typeTimer += Time.deltaTime * typewriter.charsPerSecond;
            int target = Mathf.FloorToInt(typeTimer);

            while (visibleCharacterCount < target && visibleCharacterCount < originalText.Length)
            {

                if (visibleCharacterCount < charImpactTimers.Length)
                {
                    charImpactTimers[visibleCharacterCount] = Time.time;
                }


                if (visibleCharacterCount < characterProfileIndices.Length)
                {
                    int profileIndex = characterProfileIndices[visibleCharacterCount];
                    if (profileIndex < profiles.Count)
                    {
                        if (profiles[profileIndex].typewriter.enabled)
                        {
                            PlayCharSound();
                        }
                    }
                }

                visibleCharacterCount++;
            }

            tmpText.maxVisibleCharacters = visibleCharacterCount;
        }

        void AnimateCharacters()
        {
            if (profiles.Count == 0) return;

            tmpText.ForceMeshUpdate();
            textInfo = tmpText.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                int profileIndex = characterProfileIndices[i];
                if (profileIndex >= profiles.Count) profileIndex = 0; 
                AnimationProfile profile = profiles[profileIndex];

                int mat = charInfo.materialReferenceIndex;
                int vert = charInfo.vertexIndex;

                Vector3[] verts = textInfo.meshInfo[mat].vertices;
                Color32[] colors = textInfo.meshInfo[mat].colors32;

                Vector3 offset = Vector3.zero;

                // Apply effects using the profile settings
                if (profile.impact.enabled)
                    offset.y -= CalculateImpact(i, profile.impact);

                if (profile.wave.enabled)
                    offset.y += Mathf.Sin(Time.time * profile.wave.frequency + i * profile.wave.speed) * profile.wave.amplitude;

                if (profile.shake.enabled)
                    offset += Random.insideUnitSphere * profile.shake.magnitude;

                if (profile.bounce.enabled)
                    offset.y += Mathf.Abs(Mathf.Sin(Time.time * profile.bounce.speed + i * 0.1f)) * profile.bounce.height;

                if (profile.jitter.enabled)
                    offset.x += Mathf.Sin(Time.time * profile.jitter.speed + i) * profile.jitter.magnitude;

                for (int j = 0; j < 4; j++)
                    verts[vert + j] += offset;

                Vector3 center = (verts[vert] + verts[vert + 2]) / 2f;

                if (profile.scale.enabled)
                {
                    float s = 1 + Mathf.Sin(Time.time * profile.scale.speed + i) * profile.scale.scaleMultiplier;
                    for (int j = 0; j < 4; j++)
                        verts[vert + j] = center + (verts[vert + j] - center) * s;
                }

                if (profile.rotate.enabled)
                {
                    float a = Mathf.Sin(Time.time * profile.rotate.speed + i) * profile.rotate.angle;
                    Quaternion q = Quaternion.Euler(0, 0, a);
                    for (int j = 0; j < 4; j++)
                        verts[vert + j] = center + q * (verts[vert + j] - center);
                }

                if (profile.perspective.enabled)
                {
                    float t = (float)i / (textInfo.characterCount - 1);
                    float d = 1 + profile.perspective.depthCurve.Evaluate(t) * profile.perspective.depthScale;
                    for (int j = 0; j < 4; j++)
                        verts[vert + j] = center + (verts[vert + j] - center) * d;
                }


                if (profile.rainbow.enabled)
                {
                    float hue = (Time.time * profile.rainbow.speed + i * profile.rainbow.offset) % 1f;
                    Color c = Color.HSVToRGB(hue, profile.rainbow.saturation, profile.rainbow.brightness);
                    for (int j = 0; j < 4; j++)
                        colors[vert + j] = c;
                }
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;
                tmpText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }

        float CalculateImpact(int i, ImpactSettings impact)
        {
            if (!impact.enabled) return 0f;

            if (impact.loop)
            {
                float t = (Time.time + i * 0.1f) % impact.impactDuration / impact.impactDuration;
                return impact.impactCurve.Evaluate(t) * impact.impactDistance;
            }


            if (i >= charImpactTimers.Length || i >= characterProfileIndices.Length) return 0f;

            float elapsed = (Time.time - charImpactTimers[i]) / impact.impactDuration;
            return elapsed > 1f ? 0f : impact.impactCurve.Evaluate(elapsed) * impact.impactDistance;
        }

        void PlayCharSound()
        {
            if (!audioSource || !charSound) return;
            audioSource.pitch = 1f + Random.Range(-charSoundPitchVariation, charSoundPitchVariation);
            audioSource.PlayOneShot(charSound);
        }


        string ParseAnimationProfiles(string rawText)
        {

            characterProfileIndices = new int[rawText.Length];
            System.Text.StringBuilder cleanText = new System.Text.StringBuilder();
            int currentProfileIndex = 0;
            int visibleCharIndex = 0;

            for (int i = 0; i < rawText.Length; i++)
            {

                if (rawText[i] == '[' && i + 6 < rawText.Length &&
                    rawText.Substring(i, 6).Equals("[anim=", System.StringComparison.OrdinalIgnoreCase))
                {
                    int endTag = rawText.IndexOf(']', i);
                    if (endTag != -1)
                    {
                        string tagContent = rawText.Substring(i + 6, endTag - (i + 6));

                        if (profileNameToIndex.TryGetValue(tagContent, out int newProfileIndex))
                        {
                            currentProfileIndex = newProfileIndex;
                        }
                        else
                        {
                            Debug.LogWarning($"Animation Profile '{tagContent}' not found. Using Default profile.");
                            currentProfileIndex = 0;
                        }

                        i = endTag; 
                        continue;
                    }
                }

                else if (rawText[i] == '[' && i + 6 < rawText.Length &&
                         rawText.Substring(i, 7).Equals("[/anim]", System.StringComparison.OrdinalIgnoreCase))
                {
                    currentProfileIndex = 0;
                    i += 6;
                    continue;
                }


                if (visibleCharIndex < characterProfileIndices.Length)
                {
                    characterProfileIndices[visibleCharIndex] = currentProfileIndex;
                }

                cleanText.Append(rawText[i]);
                visibleCharIndex++;
            }


            System.Array.Resize(ref characterProfileIndices, visibleCharIndex);

            return cleanText.ToString();
        }
    }
}
