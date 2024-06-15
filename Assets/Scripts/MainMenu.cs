using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private AudioManager audioManager;
    private GameController controller;
    private Slider musicSlider, sfxSlider;
    private Toggle vToggle;

    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        controller = FindObjectOfType<GameController>();
        audioManager.sounds[0].source.volume = controller.MusicVolume;
        audioManager.sounds[1].source.volume = controller.SfxVolume;
        audioManager.Play("song");
        Slider[] sliders = FindObjectsOfType<Slider>();
        foreach (Slider s in sliders)
        {
            if (s.gameObject.name == "MusicVolume")
                musicSlider = s;
            else
                sfxSlider = s;
        }
        vToggle = FindObjectOfType<Toggle>();
        SetSettings();
    }

    private void SetSettings()
    {
        musicSlider.value = controller.MusicVolume;
        sfxSlider.value = controller.SfxVolume;
        vToggle.isOn = controller.Vibrate;
    }

    public void VolumeChange(Slider s)
    {
        if (s.gameObject.name == "MusicVolume")
        {
            controller.MusicVolume = s.value;
            audioManager.sounds[0].source.volume = controller.MusicVolume;
        }
        else if (s.gameObject.name == "SfxVolume")
        {
            controller.SfxVolume = s.value;
            audioManager.sounds[1].source.volume = controller.SfxVolume;
        }
    }

    public void ToggleChange(Toggle t)
    {
        controller.Vibrate = t.isOn;
    }

    public void PlayButtonAudio()
    {
        audioManager.Play("select");
        if (controller.Vibrate) Vibration.Vibrate(30);
    }

    public void LoadGame()
    {
        controller.LoadLevel(1);
    }

    public void QuitGame()
    {
        StartCoroutine(controller.Quit());
    }
}
