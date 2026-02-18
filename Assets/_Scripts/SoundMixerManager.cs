using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour
{
    public static SoundMixerManager Instance;
    
    public AudioMixer audioMixer;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        
        SetMasterVolume(Globals.MasterVolume);
        SetSoundFXVolume(Globals.SoundFXVolume);
        SetMusicVolume(Globals.MusicVolume);
    }
    
    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20.0f);
        Globals.MasterVolume = volume;
    }

    public void SetSoundFXVolume(float volume)
    {
        audioMixer.SetFloat("SoundFXVolume", Mathf.Log10(volume) * 20.0f);
        Globals.SoundFXVolume = volume;
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20.0f);
        Globals.MusicVolume = volume;
    }
}
