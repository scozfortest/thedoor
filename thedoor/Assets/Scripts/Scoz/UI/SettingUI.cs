using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Scoz.Func;

namespace Scoz.Func {
    public class SettingUI : BaseUI {
        [SerializeField]
        Dropdown LanguageDropDown = null;
        [SerializeField]
        Slider MusicSlider = null;
        [SerializeField]
        Slider SoundSlider = null;
        [SerializeField]
        Slider VoiceSlider = null;

        private void Awake() {
            InitLanguageDropdown();
        }

        protected override void OnEnable() {
            base.OnEnable();
            Time.timeScale = 0;
            MusicSlider.value = AudioPlayer.MusicVolumeRatio;
            SoundSlider.value = AudioPlayer.SoundVolumeRatio;
            VoiceSlider.value = AudioPlayer.VoiceVolumeRatio;
            //LanguageDropDown.value = (int)GamePlayer.Instance.UsingLanguage;
        }
        protected override void OnDisable() {
            base.OnDisable();
            Time.timeScale = 1;
        }
        public void SetLanguage() {
            //GamePlayer.Instance.SetLanguage((Language)LanguageDropDown.value);
        }
        public void SetMusicVolume(float _volume) {
            AudioPlayer.SetMusicVolume(_volume);
        }
        public void SetSoundVolume(float _volume) {
            AudioPlayer.SetSoundVolume(_volume);
        }
        public void SetVoiceVolume(float _volume) {
            AudioPlayer.SetVoiceVolume(_volume);
        }
        public void FullScreen() {
            Screen.fullScreen = !Screen.fullScreen;
        }
        void InitLanguageDropdown() {
            if (!LanguageDropDown)
                return;
            LanguageDropDown.ClearOptions();
            string[] languages = Enum.GetNames(typeof(Language));
            LanguageDropDown.AddOptions(new List<string>(languages));
            //LanguageDropDown.value = (int)GamePlayer.Instance.UsingLanguage;
        }
        public void OnNextLanguageClick() {
            //Language language = MyEnum.GetNext(GamePlayer.Instance.UsingLanguage);
            //GamePlayer.Instance.SetLanguage(language);
            RefreshUI();
        }
        public void OnPreviousLanguageClick() {
            //Language language = MyEnum.GetPrevious(GamePlayer.Instance.UsingLanguage);
            //GamePlayer.Instance.SetLanguage(language);
            RefreshUI();
        }
        public void ClearData() {
            PlayerPrefs.DeleteAll();
        }

    }
}
