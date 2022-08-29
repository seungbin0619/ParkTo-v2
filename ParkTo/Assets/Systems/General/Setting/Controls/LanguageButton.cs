using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageButton : ThemeObject
{
    private static readonly Color banColor = new Color32(114, 114, 120, 255);

    [SerializeField]
    private UnityEngine.Localization.Locale target;
    private Button button;

    private bool IsInitialized { set; get; }

    protected override void Awake()
    {
        button = GetComponent<Button>();
        graphic = GetComponent<Image>();

        if (IsInitialized) return;

        UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocaleChanged
            += LocalizationSettings_SelectedLocaleChanged;

        IsInitialized = true;
    }

    private void OnDestroy()
    {
        if (!IsInitialized) return;

        UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocaleChanged
            -= LocalizationSettings_SelectedLocaleChanged;

        IsInitialized = false;
    }

    protected override void Start() => LocalizationSettings_SelectedLocaleChanged(UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale);
    protected override void OnEnable() {}

    private void LocalizationSettings_SelectedLocaleChanged(UnityEngine.Localization.Locale obj)
    {
        bool flag = obj == target;

        button.interactable = !flag;
        button.targetGraphic.color = flag ? ThemeManager.currentTheme.colors[index] : banColor;
    }
}
