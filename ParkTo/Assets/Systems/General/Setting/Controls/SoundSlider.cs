using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSlider : VolumeSlider
{
    private void OnEnable() => slider.value = SettingManager.instance?.GetSoundVolume() ?? 0.5f;
    public void SetSoundVolume(float value) => SettingManager.instance?.SetSoundVolume(slider.value);
}
