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

    protected override void Awake()
    {
        base.Awake();

        SetMusicVolume(DataManager.GetData("Game", "Music", 50) * 0.01f);
        SetSoundVolume(DataManager.GetData("Game", "Sound", 50) * 0.01f);

        SetLanguage(DataManager.GetData("Game", "Language", 0));
    }

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

        //DataManager.SaveData();
    }

    public void SetLanguage(int index)
    {
        if (!LocalizationManager.instance.SetLanguage(index)) return;

        index = LocalizationManager.Index;
        DataManager.SetData("Game", "Language", index);
    }

    public void SetMusicVolume(float value)
    {
        SFXManager.instance.SetBgmVolume(value);
        DataManager.SetData("Game", "Music", (int)(SFXManager.instance.BgmVolume * 100));
    }

    public void SetSoundVolume(float value)
    {
        SFXManager.instance.SetSoundVolume(value);
        DataManager.SetData("Game", "Music", (int)(SFXManager.instance.SoundVolume * 100));
    }

    public void Goto(string name)
    {
        ActionManager.AddAction("FadeIn", 1f);
        ActionManager.AddAction("Move", name);
        ActionManager.AddAction("FadeOut", 1f);

        ActionManager.Play();
    }
}
