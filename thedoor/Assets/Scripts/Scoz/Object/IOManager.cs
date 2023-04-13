using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Scoz.Func
{
    public class IOManager : MonoBehaviour
    {
        public static void SaveBytes(byte[] _bytes, string _path)
        {
            try
            {
                string filePath = string.Format("{0}/{1}", Application.persistentDataPath, _path);
                System.IO.FileInfo file = new System.IO.FileInfo(filePath);
                file.Directory.Create();
                System.IO.File.WriteAllBytes(filePath, _bytes);
            }
            catch (Exception _e)
            {
                WriteLog.LogError(_e);
            }

            //DebugLogger.Log(_bytes.Length / 1024 + "Kb was saved as: " + _fullPath);
        }
        public static bool CheckFileExist(string _path)
        {
            string filePath = string.Format("{0}/{1}", Application.persistentDataPath, _path);
            return System.IO.File.Exists(filePath);
        }
        /// <summary>
        /// 檢查檔案存不存在Asset底下的路徑(區分大小寫)
        /// </summary>
        public static bool CheckFileInUnityAsset(string _path)
        {
            string filePath = string.Format("{0}/{1}", Application.dataPath, _path);
            return FileExistsCaseSensitive(filePath);
        }
        static bool FileExistsCaseSensitive(string filename)
        {
            string name = Path.GetDirectoryName(filename);

            return name != null
                   && Array.Exists(Directory.GetFiles(name), s => s == Path.GetFullPath(filename));
        }
        public static void DeleteAllFiles()
        {
            foreach (var directory in Directory.GetDirectories(Application.persistentDataPath))
            {
                DirectoryInfo data_dir = new DirectoryInfo(directory);
                data_dir.Delete(true);
            }

            foreach (var file in Directory.GetFiles(Application.persistentDataPath))
            {
                FileInfo file_info = new FileInfo(file);
                file_info.Delete();
            }
        }
        public static void DeleteFIle(string _path)
        {
            string filePath = string.Format("{0}/{1}", Application.persistentDataPath, _path);
            WriteLog.Log("移除檔案:" + filePath);
            System.IO.File.Delete(filePath);
        }
        public static byte[] LoadBytes(string _path)
        {
            string filePath = string.Format("{0}/{1}", Application.persistentDataPath, _path);
            byte[] bytes = System.IO.File.ReadAllBytes(filePath);
            return bytes;
        }
        public static Sprite LoadSprite(string _path)
        {
            string filePath = string.Format("{0}/{1}", Application.persistentDataPath, _path);
            byte[] bytes = System.IO.File.ReadAllBytes(filePath);
            if (bytes == null || bytes.Length == 0)
                return null;
            Texture2D t = new Texture2D(1, 1);
            try
            {
                t.LoadImage(bytes);
            }
            catch
            {
                WriteLog.LogError("讀取Image失敗:" + _path);
                return null;
            }
            return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
        }
        public static Texture2D LoadPNGAsTexture(string _path)
        {
            string filePath = string.Format("{0}/{1}", Application.persistentDataPath, _path);
            byte[] bytes = System.IO.File.ReadAllBytes(filePath);
            Texture2D t = new Texture2D(1, 1);
            try
            {
                t.LoadImage(bytes);
            }
            catch
            {
                WriteLog.LogError("讀取Image失敗:" + _path);
                return null;
            }
            return t;
        }
        public static Sprite ChangeTextureToSprite(Texture2D _texture)
        {
            Sprite s = Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), new Vector2(0.5f, 0.5f));
            return s;
        }

        /// <summary>
        /// 存文字檔到硬碟中，非行動裝置用
        /// </summary>
        public static void SaveTextToFolder(string _fileName, string _filePath, string _text)
        {
            if (!Directory.Exists(_filePath))
            {
                Directory.CreateDirectory(_filePath);
            }
            _filePath += _fileName;
            try
            {
                File.WriteAllText(_filePath, _text);
            }
            catch (Exception _e)
            {
                string ErrorMessages = "File Write Error\n" + _e.Message;
                WriteLog.LogError(ErrorMessages);
            }
        }
    }
}
