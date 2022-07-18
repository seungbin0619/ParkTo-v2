using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyButton : ThemeObject
{
    private static readonly Color banColor = new Color32(114, 114, 120, 255);
    UnityEngine.UI.Button button;

    protected override void Awake() {
        base.Awake();

        button = GetComponent<UnityEngine.UI.Button>();
    }

    private void Update() {
        bool flag = SettingManager.instance.SelectedResolution != SettingManager.instance.GetResolution();
        flag |= SettingManager.instance.SelectedWindowMode != SettingManager.instance.GetWindowMode();

        button.interactable = flag;
        if(button.interactable) FollowTheme();
        else graphic.color = banColor;
    }
}
