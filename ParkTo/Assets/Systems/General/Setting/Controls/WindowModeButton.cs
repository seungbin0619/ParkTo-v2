using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowModeButton : MonoBehaviour
{
    static readonly FullScreenMode[] series = 
        new FullScreenMode[3] { 
            FullScreenMode.FullScreenWindow, 
            FullScreenMode.Windowed, 
            FullScreenMode.ExclusiveFullScreen 
        };
    TMPro.TMP_Text textBox;

    private void OnEnable() {
        textBox ??= GetComponentInChildren<TMPro.TMP_Text>();
        FullScreenMode index = SettingManager.instance.GetWindowMode();

        SettingManager.instance.SelectedWindowMode = index;
        textBox.text = SettingManager.instance.WindowModeToString(index);
    }

    public void ChangeDisplayMode() {
        int i;
        for(i = 0; i < series.Length; i++)
            if(series[i] == SettingManager.instance.SelectedWindowMode) break;

        FullScreenMode index = series[(i + 1) % series.Length];
        SettingManager.instance.SelectedWindowMode = index;

        textBox.text = SettingManager.instance.WindowModeToString(index);
    }
}
