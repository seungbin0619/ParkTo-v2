using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleButton : MonoBehaviour
{
    private void Start() {
        NoticeManager.instance.NoticeString("해당 버전은 게임위 심의용입니다.\n모든 스테이지가 잠금 해제됩니다.");    
    }

    public void StartGame() {
        //SFXManager.instance.PlaySound(1);
        
        int themeCount = ThemeManager.instance.themes.Count - 1;
        if(
            SelectManager.LastSelectedLevel == -1 && 
            DataManager.GetData("Game", "Theme" + themeCount, 0) >= ThemeManager.instance.themes[themeCount].levels.Count) {
            // 올클리어 한 경우?
            SelectManager.Delta = -1;
            SelectManager.NextPage = (ThemeManager.instance.themes[themeCount].levels.Count - 1) / SelectManager.MAX_COUNT;
            
            SettingManager.instance.Goto("Under Construction");
            return;
        }

        SelectManager.Delta = 1;
        SelectManager.IsFirstEnter = true;
        
        SettingManager.instance.Goto("Select");
    } 
    
#if UNITY_EDITOR
    public void ExitGame() => UnityEditor.EditorApplication.isPlaying = false;
#else
    public void ExitGame() => Application.Quit();
#endif
}
