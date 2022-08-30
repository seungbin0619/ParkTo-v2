using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class SteamApiManager : SingleTon<SteamApiManager> {
    [System.NonSerialized]
    public CGameID m_GameID;
    [System.NonSerialized]
    public AppId_t appId;
    [System.NonSerialized]
    public int userId;

#if !UNITY_EDITOR
    private void Start() {
        appId = SteamUtils.GetAppID();
        m_GameID = new CGameID(SteamUtils.GetAppID());
        
        //Debug.Log(appId);
        //Debug.Log(m_GameID);
    }
#endif

    public void ClearAchievement(string achID) {
#if !UNITY_EDITOR
        if (SteamManager.Initialized)
        if(!SteamUserStats.GetAchievement(achID, out bool achieved) || achieved) return;
        
        SteamUserStats.SetAchievement(achID);
        SteamUserStats.StoreStats();
#endif
    }

    public void CheckClearAchievements() {
        for(int i = 0; i < ThemeManager.instance.themes.Count; i++) {
            ThemeBase theme = ThemeManager.instance.themes[i];

            for(int j = 0; j < theme.levels.Count / SelectManager.MAX_COUNT; j++) {
                string aName = $"COMPLETE_{i}_{j}";
                if(SteamDataManager.GetData("Game", "Theme" + i, 0) < (j + 1) * SelectManager.MAX_COUNT) continue;

                ClearAchievement(aName);
            }
        }
    }
}
