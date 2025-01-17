using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingsHandler : MonoBehaviour
{
    public UnityEvent onConfirmedSettings;
    public Slider musicVolume;
    public Slider soundsVolume;
    public Toggle batterySaveMode;
    public void ConfirmSettings()
    {
        PlayerPrefs.SetFloat("musicVolume", musicVolume.value);
        PlayerPrefs.SetFloat("soundsVolume", soundsVolume.value);
        PlayerPrefs.SetInt("batterySaveMode", Convert.ToInt32(batterySaveMode.isOn));
        PlayerPrefs.Save();
        onConfirmedSettings.Invoke();
    }
    public void ReloadOrUpdateVisuals()
    {
        musicVolume.value = PlayerPrefs.GetFloat("musicVolume", 1f);
        soundsVolume.value = PlayerPrefs.GetFloat("soundsVolume", 1f);
        batterySaveMode.isOn = Convert.ToBoolean(PlayerPrefs.GetInt("batterySaveMode", 1));
    }
    private void OnEnable()
    {
        ReloadOrUpdateVisuals();
    }
}
