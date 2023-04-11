using TheDoor.Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scoz.Func {

    //////////////////LitJson不能吃float所以float都要改double///////////////////////////
    public struct LocoSettingData {
        public int UseLanguage;
        public double SoundVolume;
        public double MusicVolume;
        public double VoiceVolume;
        public LocoSettingData(int _useLanguage, double _soundVolume, double _musicVolume, double _voiceVolume) {
            UseLanguage = _useLanguage;
            SoundVolume = _soundVolume;
            SoundVolume = Math.Round(SoundVolume, 2);
            MusicVolume = _musicVolume;
            MusicVolume = Math.Round(MusicVolume, 2);
            VoiceVolume = _voiceVolume;
            VoiceVolume = Math.Round(VoiceVolume, 2);
        }
    }
    public struct LocoPlayerSettingData {
        public bool PostProcessing;//是否開啟後製效果
        public bool Vibration;//是否開啟震動效果

        public LocoPlayerSettingData(Dictionary<string, object> _setting) {
            PostProcessing = Convert.ToBoolean(_setting["PostProcessing"]);
            Vibration = Convert.ToBoolean(_setting["Vibration"]);
        }
    }

    public struct LocoRedDotData {
        public string[] RedDot_ShopDataUIDs;//已經觀看過的商城品項
        public int[] Icon;//已經觀看過的Icon品項


        public LocoRedDotData(Dictionary<string, object> _setting) {
            object value;
            RedDot_ShopDataUIDs = new string[0];
            try {
                RedDot_ShopDataUIDs = _setting.TryGetValue("RedDot_ShopDataUIDs", out value) ? (string[])value : new string[0];
            } catch (Exception _ex) {
                DebugLogger.LogError("LocoRedDotData>RedDot_ShopDataUIDs錯誤: " + _ex);
            }
            Icon = new int[0];
            try {
                Icon = _setting.TryGetValue(NotUniqueItemTypes.Icon.ToString(), out value) ? (int[])value : new int[0];
            } catch (Exception _ex) {
                DebugLogger.LogError("LocoRedDotData>Icon錯誤: " + _ex);
            }
        }
    }



}
