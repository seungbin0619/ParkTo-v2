using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameSettingButton : SettingButton
{
    public override void OpenSetting()
    {
        if (GameManager.instance.IsAnimate) return;
        if (HelpManager.IsInitialize) return;

        SettingManager.instance.OpenSetting();
    }
}
