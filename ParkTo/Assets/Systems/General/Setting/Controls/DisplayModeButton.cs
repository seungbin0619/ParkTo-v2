using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class DisplayModeButton : ThemeObject
{
    TMPro.TMP_Text textBox;

    protected override void OnEnable() {
        base.OnEnable();
        
        textBox ??= GetComponentInChildren<TMPro.TMP_Text>();
        LocalizationSettings.SelectedLocaleChanged
            += LocalizationSettings_SelectedLocaleChanged;

        LocalizationSettings_SelectedLocaleChanged(LocalizationSettings.SelectedLocale);
    }

    private void OnDisable() {
        LocalizationSettings.SelectedLocaleChanged
            -= LocalizationSettings_SelectedLocaleChanged;
    }

    private void LocalizationSettings_SelectedLocaleChanged(UnityEngine.Localization.Locale obj) {
        string WindowMode = SettingManager.instance.WindowModeToString(SettingManager.instance.GetWindowMode());
        string Resolution = SettingManager.instance.ResolutionToString(SettingManager.instance.GetResolution());

        textBox.text = WindowMode + " (" + Resolution + ")";
    }
}
