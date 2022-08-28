using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class SettingManager : SingleTon<SettingManager>
{
    public bool IsAnimate { set; get; }
    public bool IsOpen { private set; get; }
    public bool IsScreenSettingOpen { private set; get; }

    [SerializeField]
    private Animation hidePanel;
    public Animation Border;
    public Animation ScreenBorder;

    public int SelectedResolution { get; set; }
    public FullScreenMode SelectedWindowMode { get; set; }

    protected override void Awake()
    {
        base.Awake();

        SetMusicVolume(GetMusicVolume());
        SetSoundVolume(GetSoundVolume());
        SetLanguage(GetLanguage());
        //SetResolution(GetResolution());
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

        SFXManager.instance.PlaySound(2);
    }

    public void CloseSetting()
    {
        if(!IsOpen) return;
        if (IsAnimate) return;
        if(IsScreenSettingOpen) {
            CloseScreenSetting();
            return;
        }

        IsAnimate = true;
        IsOpen = false;

        hidePanel.Play("FadeOut");
        Border.Play("BorderHide");

        SteamDataManager.SaveData();
        SFXManager.instance.PlaySound(1);
    }

    public void OpenScreenSetting() {
        if(IsAnimate) return;
        if(IsScreenSettingOpen) return;

        IsAnimate = true;
        IsScreenSettingOpen = true;

        ScreenBorder.gameObject.SetActive(true);
        
        Border.Play("BorderHide");
        StartCoroutine(DelayedAnimation(0.5f, 
        () => {
            ScreenBorder.Play("ScreenBorderShow"); 
            return true;
        }));

        ScreenBorder.transform.SetSiblingIndex(2);
    }
    
    public void CloseScreenSetting() {
        if(IsAnimate) return;
        if(!IsScreenSettingOpen) return;

        IsAnimate = true;
        IsScreenSettingOpen = false;

        Border.gameObject.SetActive(true);

        ScreenBorder.Play("ScreenBorderHide");
        StartCoroutine(DelayedAnimation(0.5f, 
        () => {
            Border.Play("BorderShow"); 
            return true;
        }));

        ScreenBorder.transform.SetSiblingIndex(1);
        SFXManager.instance.PlaySound(1);
    }

    public void ApplyScreenSetting() {
        SetWindowMode(SelectedWindowMode);
        SetResolution(SelectedResolution);
    }

    private IEnumerator DelayedAnimation(float delay, Func<bool> function) {
        yield return YieldDictionary.WaitForSeconds(delay);
        function();
    }

    public void SetLanguage(int index)
    {
        if (!LocalizationManager.instance.SetLanguage(index)) return;

        index = LocalizationManager.Index;
        SteamDataManager.SetData("Game", "Language", index);
    }

    public int GetLanguage() => SteamDataManager.GetData("Game", "Language", 0);

    public void SetMusicVolume(float value)
    {
        SFXManager.instance.SetBgmVolume(value);
        SteamDataManager.SetData("Game", "Music", (int)(SFXManager.instance.BgmVolume * 100));
    }

    public float GetMusicVolume() => SteamDataManager.GetData("Game", "Music", 50) * 0.01f;

    public void SetSoundVolume(float value)
    {
        SFXManager.instance.SetSoundVolume(value);
        SteamDataManager.SetData("Game", "Sound", (int)(SFXManager.instance.SoundVolume * 100));
    }

    public float GetSoundVolume() => SteamDataManager.GetData("Game", "Sound", 50) * 0.01f;

    public void Goto(string name)
    {
        CloseSetting();

        ActionManager.AddAction("FadeIn", 1f);
        ActionManager.AddAction("Move", name);
        ActionManager.AddAction("FadeOut", 1f);

        ActionManager.Play();
    }

    public void SetResolution(int index = -1) {
        if(index < 0) return;
        if(GetResolution() == index) return;

        Resolution resolution = Screen.resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, (FullScreenMode)GetWindowMode());

        //DataManager.SetData("Game", "Resolution", index); <- 저장할 필요성을 못 느꼈다
    }

    public int GetResolution() {
        List<Resolution> list = new List<Resolution>();
        list.AddRange(Screen.resolutions);

        int index = list.FindIndex(p => p.width == Screen.width && p.height == Screen.height);
        if(index == -1) return Screen.resolutions.Length - 1; // 이거 가능하긴 한가?

        return index;
    }

    public Resolution GetResolutionData(int index) => Screen.resolutions[index];

    public string ResolutionToString(Resolution resolution) => resolution.width + "x" + resolution.height;
    
    public string ResolutionToString(int index) => GetResolutionData(index).width + "x" + GetResolutionData(index).height;

    public void SetWindowMode(FullScreenMode mode) {
        if(Screen.fullScreenMode == mode) return;
        Screen.fullScreenMode = mode;

        //DataManager.SetData("Game", "WindowMode", (int)mode);
    }

    public FullScreenMode GetWindowMode() => Screen.fullScreenMode;

    public string WindowModeToString(FullScreenMode mode) {
        string LocaleText(string data) 
            => LocalizationSettings.StringDatabase.GetLocalizedString("UIText", data);

        string target = "setting_window_";
        switch(mode) {
            case FullScreenMode.FullScreenWindow: target += "full"; break; // 1
            case FullScreenMode.Windowed:  target += "windowed"; break; // 0
            case FullScreenMode.ExclusiveFullScreen:  target += "exclusive"; break; // 3
            default: target += "full"; break;
        }

        return LocaleText(target);
    }
}
