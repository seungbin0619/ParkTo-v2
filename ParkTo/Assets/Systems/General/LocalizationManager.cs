using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LocalizationManager : SingleTon<LocalizationManager>
{
    [SerializeField]
    private List<Locale> locales = new List<Locale>();
    public static int Index { private set; get; }

    public bool SetLanguage(int index)
    {
        if (index < 0 || index >= locales.Count) index = 0;
        //if (index == Index) return false;

        UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale = locales[index];
        Index = index;

        return true;
    }

    public string LocaleText(string part, string id) {
        return LocalizationSettings.StringDatabase.GetLocalizedString(part, id);
    }
}
