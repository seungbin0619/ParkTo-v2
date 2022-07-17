using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : SingleTon<SettingManager>
{
    public bool IsAnimate { set; get; }
    public bool IsOpen { private set; get; }

    [SerializeField]
    private Animation hidePanel;
    public Animation Border;

    protected override void Awake()
    {
        base.Awake();

        SetMusicVolume(GetMusicVolume());
        SetSoundVolume(GetSoundVolume());
        SetLanguage(GetLanguage());
        //SetResolution(DataManager.GetData("Game", "Screen", 0));
    }

    public void OpenSetting()
    {
        if(IsOpen) return;
        if (IsAnimate) return;

        IsAnimate = true;
        IsOpen = true;

        Border.gameObject.SetActive(true);
        if (!hidePanel.gameObject.activeSelf)
            hidePanel.gameObject.SetActive(true);

        hidePanel.Play("FadeIn");
        Border.Play("BorderShow");
    }

    public void CloseSetting()
    {
        if(!IsOpen) return;
        if (IsAnimate) return;

        IsAnimate = true;
        IsOpen = false;

        hidePanel.Play("FadeOut");
        Border.Play("BorderHide");

        DataManager.SaveData();
    }

    public void SetLanguage(int index)
    {
        if (!LocalizationManager.instance.SetLanguage(index)) return;

        index = LocalizationManager.Index;
        DataManager.SetData("Game", "Language", index);
    }

    public int GetLanguage() => DataManager.GetData("Game", "Language", 0);

    public void SetMusicVolume(float value)
    {
        SFXManager.instance.SetBgmVolume(value);
        DataManager.SetData("Game", "Music", (int)(SFXManager.instance.BgmVolume * 100));
    }

    public float GetMusicVolume() => DataManager.GetData("Game", "Music", 50) * 0.01f;

    public void SetSoundVolume(float value)
    {
        SFXManager.instance.SetSoundVolume(value);
        DataManager.SetData("Game", "Sound", (int)(SFXManager.instance.SoundVolume * 100));
    }

    public float GetSoundVolume() => DataManager.GetData("Game", "Sound", 50) * 0.01f;

    public void Goto(string name)
    {
        CloseSetting();

        ActionManager.AddAction("FadeIn", 1f);
        ActionManager.AddAction("Move", name);
        ActionManager.AddAction("FadeOut", 1f);

        ActionManager.Play();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            if (!IsOpen) OpenSetting();
            else CloseSetting();
    }
}
