using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Security.Cryptography;
using UnityEngine;

public class SteamDataManager : SingleTon<SteamDataManager>
{
    private Dictionary<string, Dictionary<string, int>> data;

    [SerializeField]
    private string[] parts;

    [SerializeField]
    private bool crypto;

    [SerializeField]
    private string password;

    [SerializeField]
    private bool dontSave = false;

    protected override void Awake()
    {
        base.Awake();
        
        if(!SteamManager.Initialized) {
            NoticeManager.instance.NoticeString(LocalizationManager.instance.LocaleText("UIText", "notice_steam_init_error"));
            // 스팀 연동 오류
            data = LoadData(true);
            DataManager.instance.data = data;

            return;
        }

        data = LoadData();

        uint steamid = (uint)Steamworks.SteamUser.GetSteamID().GetAccountID();
        SetData("Game", "steamid", (int)steamid);

        //SetData("Game", "Theme0", 15);
    }

    public static void Load(bool flag = false) => instance.data = LoadData(flag);

    public static void SetData(string part, string key, int value) => instance.data[part][key] = value;

    public static bool HasData(string part, string key) => instance.data[part].ContainsKey(key);

    public static int GetData(string part, string key, int def = -1)
    {
        if (!instance.data[part].ContainsKey(key)) return def;
        return instance.data[part][key];
    }

    public static void SaveData()
    {
        if(instance.dontSave) return;
        Dictionary<string, string> data = GetSaveData();

        if(!SteamManager.Initialized) {
            DataManager.SaveData();
            return;
        }

        Steamworks.SteamRemoteStorage.BeginFileWriteBatch(); 

        foreach (var d in data) {
            byte[] bytesData = System.Text.Encoding.UTF8.GetBytes(d.Value);
            Steamworks.SteamRemoteStorage.FileWriteAsync(d.Key, bytesData, (uint)bytesData.Length); 
        }

        Steamworks.SteamRemoteStorage.EndFileWriteBatch();
    }

    public static Dictionary<string, string> GetSaveData()
    {
        if(instance.dontSave) return new Dictionary<string, string>();

        Dictionary<string, string> data =  new Dictionary<string, string>();
        for (int i = 0; i < instance.parts.Length; i++)
        {
            string part = instance.parts[i];
            XElement el = new XElement("root", instance.data[part].Select(kv => new XElement(kv.Key, kv.Value)));

            string encryptData = el.ToString();
            if(instance.crypto) encryptData = AESCrypto.Encrypt(encryptData, instance.password);

            data[part + ".xml"] = encryptData;
        }

        return data;
    }

    public static Dictionary<string, Dictionary<string, int>> LoadData(bool flag = false) {
        var data = GetLoadData(flag);

        if(data == null) {
            // 데이터를 불러오는 중 오류 발생
            NoticeManager.instance.NoticeString(LocalizationManager.instance.LocaleText("UIText", "notice_data_load_error"));
            
            instance.dontSave = true;
            return GetLoadData(true);
        }

        if(SteamManager.Initialized && (!data["Game"].TryGetValue("steamid", out int steamid) || 
           (uint)steamid != (uint)Steamworks.SteamUser.GetSteamID().GetAccountID()))
            return GetLoadData(true);

        return data;
    }
    private static Dictionary<string, Dictionary<string, int>> GetLoadData(bool flag = false)
    {
        var data = new Dictionary<string, Dictionary<string, int>>();
        for (int i = 0; i < instance.parts.Length; i++)
        {
            string part = instance.parts[i];
            string partPath = part + ".xml";
            data[part] = new Dictionary<string, int>();

            if(flag) continue;
            //Debug.Log(partPath + " " + Steamworks.SteamRemoteStorage.FileExists(partPath));
            if (Steamworks.SteamRemoteStorage.FileExists(partPath))
            {
                try
                {
                    int byteSize = Steamworks.SteamRemoteStorage.GetFileSize(partPath);
                    byte[] readData = new byte[byteSize];
                    Steamworks.SteamRemoteStorage.FileRead(partPath, readData, readData.Length);

                    string decryptData = System.Text.Encoding.Default.GetString(readData);
                    if (instance.crypto) decryptData = AESCrypto.Decrypt(decryptData, instance.password);

                    //Debug.Log(decryptData);

                    XElement root = XElement.Parse(decryptData);

                    foreach (var element in root.Elements()) {
                        data[part].Add(element.Name.LocalName, int.Parse(element.Value));
                    }
                }catch
                {
                    return null;
                }
            } else return null;
        }

        return data;
    }
}