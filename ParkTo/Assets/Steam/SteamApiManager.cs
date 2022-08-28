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
#if UNITY_EDITOR
        return;
#endif
        if (SteamManager.Initialized)

        SteamUserStats.SetAchievement(achID);
        SteamUserStats.StoreStats();
    }

    public void SteamCloudSave() {
        
    }
}
