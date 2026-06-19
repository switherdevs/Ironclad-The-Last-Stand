using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    public Slider volumeSlider;
    public AudioSource musicSource;

    void Start()
    {
        volumeSlider.value = musicSource.volume;
        volumeSlider.onValueChanged.AddListener(ChangeVolume);
    }

    public void ChangeVolume(float value)
    {
        musicSource.volume = value;
    }
}