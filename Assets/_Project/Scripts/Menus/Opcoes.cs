using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Opcoes : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider SoundSlider;
    public Toggle fullScreen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSoundVolume(float volume)
    {
        if (volume <= 0.0001f)
        {
        audioMixer.SetFloat("Master", -80f); // Mudo
        }
        else
        {
        audioMixer.SetFloat("Master", Mathf.Log10(volume) * 20f);
        }
    }

    public void SetFullScreen()
    {
        if (fullScreen.isOn)
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            Screen.fullScreen = true;
            Screen.SetResolution(1920, 1080, true);
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            Screen.fullScreen = false;
            Screen.SetResolution(1280, 720, false);
        }
    }
}
