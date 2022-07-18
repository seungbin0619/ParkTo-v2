using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingButton : MonoBehaviour
{
    public virtual void OpenSetting()
    {
        SettingManager.instance.OpenSetting();
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            if(!SettingManager.instance.IsOpen) OpenSetting();
            else SettingManager.instance.CloseSetting();
        }
    }
}
