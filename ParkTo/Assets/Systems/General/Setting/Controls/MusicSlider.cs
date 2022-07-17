using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSlider : VolumeSlider
{
    private void OnEnable() => slider.value = SettingManager.instance.GetMusicVolume();
    public void SetMusicVolume(float value) => SettingManager.instance.SetMusicVolume(slider.value);
}
