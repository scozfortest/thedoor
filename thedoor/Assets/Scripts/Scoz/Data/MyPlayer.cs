using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SimpleJSON;
using TheDoor.Main;

namespace Scoz.Func {
    public abstract class MyPlayer {
        public static bool IsInit = false;
        public static MyPlayer Instance {
            get {
                return GamePlayer.Instance;
            }
        }
        public Language UsingLanguage { get; protected set; } = Language.TW;//語系
        public bool PostProcessing { get; private set; } = false;//是否開啟後製效果(預設關閉)
        public bool Vibration { get; private set; } = true;//是否開啟震動效果(預設開啟)
        public Dictionary<string, long> UIDDic = new Dictionary<string, long>();//UID管理器 單機遊戲需要控管玩家擁有物件的UID不重複使用

        /// <summary>
        /// 取的本機Setting資料
        /// </summary>
        public virtual void LoadLocoData() {
            LoadSettingFromLoco();
        }
        public void LoadSettingFromLoco() {
            string json = LocoDataManager.GetDataFromLoco(LocoDataName.PlayerSetting);
            if (!string.IsNullOrEmpty(json)) {
                JSONNode jsNode = JSON.Parse(json);
                UsingLanguage = (Language)jsNode["UseLanguage"].AsInt;
                AudioPlayer.SetSoundVolume(jsNode["SoundVolume"].AsFloat);
                AudioPlayer.SetMusicVolume(jsNode["MusicVolume"].AsFloat);
                AudioPlayer.SetVoiceVolume(jsNode["VoiceVolume"].AsFloat);
                PostProcessing = jsNode["PostProcessing"].AsBool;
                Vibration = jsNode["Vibration"].AsBool;
                UIDDic.Clear();
                foreach (var key in jsNode["UIDDic"].Keys) {
                    UIDDic[key] = jsNode["UIDDic"][key].AsLong;
                }

            } else {
                SetLanguage((Language)GameSettingData.GetInt(GameSetting.DefaultLanguage));
                AudioPlayer.SetSoundVolume(GameSettingData.GetFloat(GameSetting.DefaultSound));
                AudioPlayer.SetMusicVolume(GameSettingData.GetFloat(GameSetting.DefaultMusic));
                AudioPlayer.SetVoiceVolume(GameSettingData.GetFloat(GameSetting.DefaultVoice));
                PostProcessing = GameSettingData.GetBool(GameSetting.PostProcessing);
                Vibration = GameSettingData.GetBool(GameSetting.Vibration);
                UIDDic.Clear();
            }
        }
        public void SaveSettingToLoco() {
            JSONObject jsObj = new JSONObject();
            jsObj.Add("UseLanguage", (int)UsingLanguage);
            jsObj.Add("SoundVolume", AudioPlayer.SoundVolumeRatio);
            jsObj.Add("MusicVolume", AudioPlayer.MusicVolumeRatio);
            jsObj.Add("VoiceVolume", AudioPlayer.VoiceVolumeRatio);
            jsObj.Add("PostProcessing", PostProcessing);
            jsObj.Add("Vibration", Vibration);
            JSONObject uidObj = new JSONObject();
            foreach (string key in UIDDic.Keys)
                uidObj.Add(key, UIDDic[key]);
            jsObj.Add("UIDDic", uidObj);
            LocoDataManager.SaveDataToLoco(LocoDataName.PlayerSetting, jsObj.ToString());
        }

        /// <summary>
        /// 取得新的UID
        /// </summary>
        public string GetNextUID(string _key) {
            if (UIDDic.ContainsKey(_key)) return (++UIDDic[_key]).ToString();
            UIDDic.Add(_key, 0);
            return UIDDic[_key].ToString();
        }

        public void SetLanguage(Language _value) {
            if (_value != UsingLanguage) {
                UsingLanguage = _value;
                MyText.RefreshActiveTexts();//刷新MyText
            }
        }
        public void SetPostProcessing(bool _on) {
            PostProcessing = _on;
        }
        public void SetVibration(bool _on) {
            Vibration = _on;
        }
    }
}