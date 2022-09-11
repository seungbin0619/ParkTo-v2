using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThemeManager : SingleTon<ThemeManager>
{
    public List<ThemeBase> themes = new List<ThemeBase>();

    public static int index = -1;
    public static ThemeBase currentTheme = null;

    protected override void Awake()
    {
        base.Awake();
        if(!isInstance) return;

        for(int i = themes.Count - 1; i >= 0; i--)
            if(DataManager.GetData("Game", "Theme" + i, 0) > 0 ||
             (i > 0 && DataManager.GetData("Game", "Theme" + (i - 1), 0) == themes[i - 1].levels.Count)) {
                SetTheme(i);
                break;
            }
            
        if(index == -1)
            SetTheme(DataManager.GetData("Game", "CurrentTheme", 0));
    }

    public void SetTheme(int index)
    {
        if (index < 0 || index >= themes.Count) return;
        if (ThemeManager.index == index) return;

        SFXManager.instance.PlayBgm(index, replay: true);
        ThemeManager.index = index;
        currentTheme = themes[index];
    }
}
