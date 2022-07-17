using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleButton : MonoBehaviour
{
    public void StartGame() => SettingManager.instance.Goto("Select");
    
#if UNITY_EDITOR
    public void ExitGame() => UnityEditor.EditorApplication.isPlaying = false;
#else
    public void ExitGame() => Application.Quit();
#endif
}
