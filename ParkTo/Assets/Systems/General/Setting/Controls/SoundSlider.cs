using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSlider : VolumeSlider
{
    private void OnEnable() => slider.value = SettingManager.instance.GetSoundVolume();
    public void SetSoundVolume(float value) => SettingManager.instance.SetSoundVolume(slider.value);
}
