using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleButton : MonoBehaviour
{
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

    private float cheatProgress = 0;
    private bool cheated = false;

    private void Update() {
        if(cheated) return;
        if(!Input.GetKey(KeyCode.S)) {
            cheatProgress = 0;
            return;
        }   

        cheatProgress += Time.deltaTime;
        if(cheatProgress < 3) return;

        cheated = true;
        for(int i = 0; i < ThemeManager.instance.themes.Count; i++)
            DataManager.SetData("Game", "Theme" + i, ThemeManager.instance.themes[i].levels.Count);

        DataManager.SaveData();
    }
    
#if UNITY_EDITOR
    public void ExitGame() => UnityEditor.EditorApplication.isPlaying = false;
#else
    public void ExitGame() => Application.Quit();
#endif
}
