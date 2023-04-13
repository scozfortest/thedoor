using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Scoz.Func;
using System.Linq;
using UnityEngine.SceneManagement;

namespace TheDoor.Main {
    public class GameFileManager {

        static Dictionary<string, Sprite> LocalSpriteDic = new Dictionary<string, Sprite>();//存在本機的圖片
        /// <summary>
        /// 傳入多個位置 先把圖載到本機
        /// </summary>
        public static void PreDownloadStorage(List<string> _paths, Action _cb) {
            _paths = _paths.Distinct().ToList();
            int downloadCount = _paths.Count;
            for (int i = 0; i < _paths.Count; i++) {
                GetLocalOrFiresorageImage(_paths[i], sprite => {
                    downloadCount--;
                    if (downloadCount == 0)
                        _cb?.Invoke();
                });
            }
        }

        /// <summary>
        /// 取FireStorage上的圖片後載入記憶體並存在本機上，之後遊戲重開只要是同個路徑都會載入本機的檔案
        /// </summary>
        public static void GetLocalOrFiresorageImage(string _path, Action<Sprite> _cb) {
            if (string.IsNullOrEmpty(_path))
                return;
            if (LocalSpriteDic.ContainsKey(_path)) {
                _cb?.Invoke(LocalSpriteDic[_path]);
            } else {
                if (IOManager.CheckFileExist(_path)) {
                    //DebugLogger.Log("取本機圖片:" + _path);
                    Sprite s = IOManager.LoadSprite(_path);
                    _cb?.Invoke(s);
                    return;
                }
            }

            WriteLog.Log("下載圖片");
            FirebaseManager.DownloadFile(_path, bytes => {
                if (bytes != null) {
                    IOManager.SaveBytes(bytes, _path);
                    Sprite s = SpriteConvert.GetSprite(bytes);
                    LocalSpriteDic.Add(_path, s);
                    _cb?.Invoke(s);
                } else {
                    WriteLog.LogErrorFormat("載不到圖片:{0}", _path);
                }
            });
        }
    }
}
