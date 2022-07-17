using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderAnimate : MonoBehaviour
{
    public void OnAnimateExit()
    {
        SettingManager.instance.IsAnimate = false;
        SettingManager.instance.Border.gameObject.SetActive(SettingManager.instance.IsOpen);
    }
}
