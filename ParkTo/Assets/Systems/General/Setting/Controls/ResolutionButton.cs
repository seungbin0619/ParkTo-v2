using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionButton : MonoBehaviour
{
    TMPro.TMP_Text textBox;

    private void OnEnable() {
        textBox ??= GetComponentInChildren<TMPro.TMP_Text>();
        textBox.text = SettingManager.instance.ResolutionToString(
            SettingManager.instance.GetResolution());
        
        SettingManager.instance.SelectedResolution = SettingManager.instance.GetResolution();
    }

    public void ChangeResolution() {
        int index = (SettingManager.instance.SelectedResolution + Screen.resolutions.Length - 1) % Screen.resolutions.Length;
        SettingManager.instance.SelectedResolution = index;

        textBox.text = SettingManager.instance.ResolutionToString(
            SettingManager.instance.SelectedResolution);
    }
}
