using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MahjongProject
{
    /// <summary>
    /// 数据持久化工具
    /// </summary>
    public static class DataStorage
    {
        private static readonly string SAVE_PATH = Application.persistentDataPath + "/SaveData/";
        private static readonly string ENCRYPTION_KEY = "MahjongGame2024";

        static DataStorage()
        {
            // 确保存档目录存在
            if (!Directory.Exists(SAVE_PATH))
            {
                Directory.CreateDirectory(SAVE_PATH);
            }
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        public static void SaveData<T>(string key, T data, bool useEncryption = true)
        {
            string json = JsonUtility.ToJson(data);
            
            if (useEncryption)
            {
                json = EncryptString(json);
            }

            string filePath = SAVE_PATH + key + ".json";
            File.WriteAllText(filePath, json);

            // 同时保存到PlayerPrefs作为备份
            PlayerPrefs.SetString(Constants.GameSettings.SAVE_KEY_PREFIX + key, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        public static T LoadData<T>(string key, bool useEncryption = true) where T : new()
        {
            string filePath = SAVE_PATH + key + ".json";
            string json = "";

            // 尝试从文件加载
            if (File.Exists(filePath))
            {
                json = File.ReadAllText(filePath);
            }
            // 如果文件不存在，尝试从PlayerPrefs加载
            else
            {
                json = PlayerPrefs.GetString(Constants.GameSettings.SAVE_KEY_PREFIX + key, "");
            }

            // 如果没有数据，返回默认值
            if (string.IsNullOrEmpty(json))
            {
                return new T();
            }

            // 解密数据
            if (useEncryption)
            {
                try
                {
                    json = DecryptString(json);
                }
                catch
                {
                    Debug.LogError($"数据解密失败：{key}");
                    return new T();
                }
            }

            // 反序列化数据
            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch
            {
                Debug.LogError($"数据反序列化失败：{key}");
                return new T();
            }
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        public static void DeleteData(string key)
        {
            string filePath = SAVE_PATH + key + ".json";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            PlayerPrefs.DeleteKey(Constants.GameSettings.SAVE_KEY_PREFIX + key);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 检查数据是否存在
        /// </summary>
        public static bool HasData(string key)
        {
            string filePath = SAVE_PATH + key + ".json";
            return File.Exists(filePath) || 
                   PlayerPrefs.HasKey(Constants.GameSettings.SAVE_KEY_PREFIX + key);
        }

        /// <summary>
        /// 清除所有数据
        /// </summary>
        public static void ClearAllData()
        {
            if (Directory.Exists(SAVE_PATH))
            {
                Directory.Delete(SAVE_PATH, true);
                Directory.CreateDirectory(SAVE_PATH);
            }
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 加密字符串
        /// </summary>
        private static string EncryptString(string text)
        {
            byte[] key = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    // 写入IV
                    msEncrypt.Write(aes.IV, 0, aes.IV.Length);

                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(text);
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        /// <summary>
        /// 解密字符串
        /// </summary>
        private static string DecryptString(string cipherText)
        {
            byte[] fullCipher = Convert.FromBase64String(cipherText);
            byte[] key = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;

                // 从密文中提取IV
                byte[] iv = new byte[aes.BlockSize / 8];
                byte[] cipher = new byte[fullCipher.Length - iv.Length];
                Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipher))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// 备份数据
        /// </summary>
        public static void BackupData(string key)
        {
            string filePath = SAVE_PATH + key + ".json";
            string backupPath = SAVE_PATH + key + "_backup.json";

            if (File.Exists(filePath))
            {
                File.Copy(filePath, backupPath, true);
            }
        }

        /// <summary>
        /// 恢复备份数据
        /// </summary>
        public static bool RestoreBackup(string key)
        {
            string filePath = SAVE_PATH + key + ".json";
            string backupPath = SAVE_PATH + key + "_backup.json";

            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, filePath, true);
                return true;
            }
            return false;
        }
    }
} 