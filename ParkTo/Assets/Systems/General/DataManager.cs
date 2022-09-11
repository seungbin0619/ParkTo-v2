using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Security.Cryptography;
using UnityEngine;

public class DataManager : SingleTon<DataManager>
{
#if UNITY_EDITOR
    private static readonly string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + @"\";
#else
    private static readonly string path = Application.persistentDataPath + @"\";
#endif

    [System.NonSerialized]
    public Dictionary<string, Dictionary<string, int>> data;

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

        data = LoadData();
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
#if UNITY_EDITOR
        //return;
#endif

        for (int i = 0; i < instance.parts.Length; i++)
        {
            string part = instance.parts[i];
            XElement el = new XElement("root", instance.data[part].Select(kv => new XElement(kv.Key, kv.Value)));
            //el.Save(path + part + ".xml");

            string encryptData = el.ToString();
            if(instance.crypto) encryptData = AESCrypto.Encrypt(encryptData, instance.password);

            File.WriteAllText(path + part + ".xml", encryptData);
        }
    }

    private static Dictionary<string, Dictionary<string, int>> LoadData(bool flag = false)
    {
        var data = new Dictionary<string, Dictionary<string, int>>();
        for (int i = 0; i < instance.parts.Length; i++)
        {
            string part = instance.parts[i];
            string partPath = path + part + ".xml";
            data[part] = new Dictionary<string, int>();

            if (flag) continue;
            if (File.Exists(partPath))
            {
                try
                {
                    string decryptData = File.ReadAllText(partPath);
                    if (instance.crypto) decryptData = AESCrypto.Decrypt(decryptData, instance.password);

                    XElement root = XElement.Parse(decryptData);

                    foreach (var element in root.Elements())
                        data[part].Add(element.Name.LocalName, int.Parse(element.Value));
                }catch
                {
                    return LoadData(true);
                }
            }
        }

        return data;
    }
}

public static class AESCrypto
{
    public static string Encrypt(string InputText, string Password)
    {
        RijndaelManaged RijndaelCipher = new RijndaelManaged();

        byte[] PlainText = System.Text.Encoding.Unicode.GetBytes(InputText);
        byte[] Salt = System.Text.Encoding.ASCII.GetBytes(Password.Length.ToString());

        PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(Password, Salt);

        ICryptoTransform Encryptor = RijndaelCipher.CreateEncryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));

        MemoryStream memoryStream = new MemoryStream();
        CryptoStream cryptoStream = new CryptoStream(memoryStream, Encryptor, CryptoStreamMode.Write);

        cryptoStream.Write(PlainText, 0, PlainText.Length);
        cryptoStream.FlushFinalBlock();

        byte[] CipherBytes = memoryStream.ToArray();

        memoryStream.Close();
        cryptoStream.Close();

        string EncryptedData = System.Convert.ToBase64String(CipherBytes);

        return EncryptedData;
    }

    public static string Decrypt(string InputText, string Password)
    {
        RijndaelManaged RijndaelCipher = new RijndaelManaged();

        byte[] EncryptedData = System.Convert.FromBase64String(InputText);
        byte[] Salt = System.Text.Encoding.ASCII.GetBytes(Password.Length.ToString());

        PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(Password, Salt);

        ICryptoTransform Decryptor = RijndaelCipher.CreateDecryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));
        MemoryStream memoryStream = new MemoryStream(EncryptedData);
        CryptoStream cryptoStream = new CryptoStream(memoryStream, Decryptor, CryptoStreamMode.Read);

        byte[] PlainText = new byte[EncryptedData.Length];

        int DecryptedCount = cryptoStream.Read(PlainText, 0, PlainText.Length);
        memoryStream.Close();
        cryptoStream.Close();

        string DecryptedData = System.Text.Encoding.Unicode.GetString(PlainText, 0, DecryptedCount);

        return DecryptedData;
    }
}
