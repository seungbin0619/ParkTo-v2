using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : SingleTon<SettingManager>
{
    public bool IsAnimate { set; get; }

    [SerializeField]
    private Animation hidePanel;

    [SerializeField]
    private Animation Border;

    public void OpenSetting()
    {
        if (IsAnimate) return;
        IsAnimate = true;

        if(!hidePanel.gameObject.activeSelf)
            hidePanel.gameObject.SetActive(true);

        hidePanel.Play("FadeIn");
        Border.Play("BorderShow");
    }

    public void CloseSetting()
    {
        if (IsAnimate) return;
        IsAnimate = true;

        hidePanel.Play("FadeOut");
        Border.Play("BorderHide");
    }
}
