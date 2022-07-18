using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderAnimate : MonoBehaviour
{
    public void OnAnimateExit()
    {
        if(!SettingManager.instance.IsScreenSettingOpen) SettingManager.instance.IsAnimate = false;
        SettingManager.instance.Border.gameObject.SetActive(
            SettingManager.instance.IsOpen ^ SettingManager.instance.IsScreenSettingOpen);
    }

    public void OnScreenBorderAnimateExit()
    {
        if(SettingManager.instance.IsScreenSettingOpen) 
            SettingManager.instance.IsAnimate = false;
        SettingManager.instance.ScreenBorder.gameObject.SetActive(SettingManager.instance.IsScreenSettingOpen);
    }
}
