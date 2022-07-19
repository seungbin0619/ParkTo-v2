using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleButton : MonoBehaviour
{
    public void StartGame() {
        //SFXManager.instance.PlaySound(1);

        SelectManager.Delta = 1;
        //SelectManager.LastSelectedLevel = SelectManager.MAX_COUNT - 1;
        SelectManager.IsFirstEnter = true;
        
        SettingManager.instance.Goto("Select");
    } 
    
#if UNITY_EDITOR
    public void ExitGame() => UnityEditor.EditorApplication.isPlaying = false;
#else
    public void ExitGame() => Application.Quit();
#endif
}
