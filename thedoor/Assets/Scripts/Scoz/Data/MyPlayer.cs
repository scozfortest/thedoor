using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LitJson;
using TheDoor.Main;

namespace Scoz.Func {
    public abstract class MyPlayer {
        public static bool IsInit = false;
        public static MyPlayer Instance {
            get {
                return GamePlayer.Instance;
            }
        }
        public Language UsingLanguage { get; protected set; } = Language.TW;

        public virtual void GetLocoData() {
            //取的本機Setting資料
            LoadSettingFromLoco();
        }
        void LoadSettingFromLoco() {
            string str = "";
            if (PlayerPrefs.HasKey(LocoData.Setting.ToString())) {
                str = PlayerPrefs.GetString(LocoData.Setting.ToString());
                JsonData item = JsonMapper.ToObject(str);
                foreach (string key in item.Keys) {
                    switch (key) {
                        case "UseLanguage":
                            UsingLanguage = ((Language)int.Parse(item[key].ToString()));
                            break;
                        case "SoundVolume":
                            AudioPlayer.SetSoundVolume(float.Parse(item[key].ToString()));
                            break;
                        case "MusicVolume":
                            AudioPlayer.SetMusicVolume(float.Parse(item[key].ToString()));
                            break;
                        case "VoiceVolume":
                            AudioPlayer.SetVoiceVolume(float.Parse(item[key].ToString()));
                            break;
                        default:
                            WriteLog.LogWarning(string.Format("LocoSettingData有不明屬性:{0}", key));
                            break;
                    }
                }
            } else {
                SetLanguage((Language)GameSettingData.GetInt(GameSetting.DefaultLanguage));
                AudioPlayer.SetSoundVolume(GameSettingData.GetFloat(GameSetting.DefaultSound));
                AudioPlayer.SetMusicVolume(GameSettingData.GetFloat(GameSetting.DefaultMusic));
                AudioPlayer.SetVoiceVolume(GameSettingData.GetFloat(GameSetting.DefaultVoice));
                return;
            }
        }
        public string GetSettingJsonStr() {
            LocoSettingData data = new LocoSettingData((int)UsingLanguage, AudioPlayer.SoundVolumeRatio, AudioPlayer.MusicVolumeRatio, AudioPlayer.VoiceVolumeRatio);
            JsonData jd = JsonMapper.ToJson(data);
            return jd.ToString();
        }
        public void SaveSettingToLoco() {
            string str = GetSettingJsonStr();
            WriteLog.LogFormat("<color=Orange>{0}</color>", str);
            PlayerPrefs.SetString(LocoData.Setting.ToString(), str);
            WriteLog.LogFormat("<color=Orange>{0}</color>", "Save Setting To Loco");
        }
        public void SetLanguage(Language _value) {
            if (_value != UsingLanguage) {
                UsingLanguage = _value;
                MyText.RefreshActiveTexts();//刷新MyText
            }
        }
    }
}