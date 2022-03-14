using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeSlider : MonoBehaviour
{
    public void SetMusicVolume(float value)
    {
        SettingManager.instance.SetMusicVolume(value);
    }

    public void SetSoundVolume(float value)
    {
        SettingManager.instance.SetSoundVolume(value);
    }
}
