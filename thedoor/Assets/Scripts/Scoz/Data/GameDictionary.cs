using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

namespace Scoz.Func {
    public partial class GameDictionary : MonoBehaviour {

        [SerializeField]
        List<Font> SysFonts = null;


        public static GameDictionary Instance;

        public static bool IsInit { get; private set; }



        public static GameDictionary CreateNewInstance() {
            if (Instance != null) {
                WriteLog.LogError("GameDictionary之前已經被建立了");
            } else {
                GameObject prefab = Resources.Load<GameObject>("Prefabs/Common/GameDictionary");
                GameObject go = Instantiate(prefab);
                go.name = "GameDictionary";
                Instance = go.GetComponent<GameDictionary>();
                Instance.InitDic();
            }
            return Instance;
        }

        public static Font GetUsingLanguageFont() {
            if (Instance == null)
                return Resources.GetBuiltinResource<Font>("Arial.ttf");
            else {
                int index = (int)MyPlayer.Instance.UsingLanguage;
                if (index < Instance.SysFonts.Count && Instance.SysFonts[index] != null)
                    return Instance.SysFonts[index];
            }
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        /// <summary>
        /// 設定字典
        /// </summary>
        public void InitDic() {
            if (IsInit)
                return;
            Instance = this;
            IsInit = true;
            LoadLocalJson();//初始化後先載一份本機string跟GameSetting
            DontDestroyOnLoad(gameObject);
        }
        /// <summary>
        /// 讀取本機的String表
        /// </summary>
        void LoadLocalJson() {
            StringDic = StringData.GetStringDic("String");
            GameSettingData.ClearStaticDic();
            StrKeyJsonDic["GameSetting"] = MyJsonData.GetDataStringKey<GameSettingData>("GameSetting");
        }
    }
}
