using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThemeManager : SingleTon<ThemeManager>
{
    public List<ThemeBase> themes = new List<ThemeBase>();

    public static int index = -1;
    public static ThemeBase currentTheme = null;

    public void SetTheme(int index)
    {
        if (index < 0 || index >= themes.Count) return;
        if (ThemeManager.index == index) return;

        ThemeManager.index = index;
        currentTheme = themes[index];
    }
}
