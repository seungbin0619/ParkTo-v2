using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnableButton : ThemeObject
{
    private static Color32 unableColor = new Color32(114, 114, 120, 255);

    [SerializeField]
    string targetScene;
    UnityEngine.UI.Button button;

    protected override void Awake()
    {
        base.Awake();

        button = GetComponent<UnityEngine.UI.Button>();
        graphic = GetComponent<UnityEngine.UI.Image>();
    }

    protected override void OnEnable() {
        button.interactable = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != targetScene;

        base.OnEnable();
    }

    protected override void FollowTheme()
    {
        if(button.interactable)
            graphic.color = ThemeManager.currentTheme.colors[index];
        else graphic.color = unableColor;
    }
}
