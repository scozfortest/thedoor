using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;

namespace TheDoor.Main {
    public class SettingUI : BaseUI {
        [SerializeField] Slider MusicSlider = null;
        [SerializeField] Slider SoundSlider = null;
        [SerializeField] Slider VoiceSlider = null;
        [SerializeField] Image MusicMuteImg;
        [SerializeField] Image SoundMuteImg;
        [SerializeField] Image VoiceMuteImg;
        [SerializeField] Text MusicValueText;
        [SerializeField] Text SoundValueText;
        [SerializeField] Text VoiceValueText;
        [SerializeField] Toggle PostProcessingToggle;
        [SerializeField] Toggle VibrationToggle;
        [SerializeField] Text PlayerACText;
        [SerializeField] Text VersionText;

        //語系選擇
        [SerializeField] GameObject LanguageSelectGO;
        [SerializeField] GameObject TWGO_On;
        [SerializeField] GameObject TWGO_Off;
        [SerializeField] GameObject CHGO_On;
        [SerializeField] GameObject CHGO_Off;

        //三方登入
        [SerializeField] GameObject Google_BlackCover;
        [SerializeField] GameObject Google_Glow;
        [SerializeField] Text Google_Text;
        [SerializeField] GameObject FB_BlackCover;
        [SerializeField] GameObject FB_Glow;
        [SerializeField] Text FB_Text;
        [SerializeField] GameObject Apple_BlackCover;
        [SerializeField] GameObject Apple_Glow;
        [SerializeField] Text Apple_Text;
        [SerializeField] GameObject AppleGO;




        Language CurLanguage;

        protected override void OnEnable() {
            base.OnEnable();
            PlayerACText.text = GamePlayer.Instance.Data.UID;
            MusicSlider.value = AudioPlayer.MusicVolumeRatio;
            SoundSlider.value = AudioPlayer.SoundVolumeRatio;
            VoiceSlider.value = AudioPlayer.VoiceVolumeRatio;
            CurLanguage = GamePlayer.Instance.UsingLanguage;
            PostProcessingToggle.isOn = GamePlayer.Instance.PostProcessing;
            VibrationToggle.isOn = GamePlayer.Instance.Vibration;
            VersionText.text = "Version: " + Application.version;
            LanguageSelectGO.SetActive(false);
            RefreshSelectLanguageUI();
            RefreshAudioUI();
            RefreshThridpartLinkUI();
        }
        public void SetMusicVolume(float _volume) {
            AudioPlayer.SetMusicVolume(_volume);
            RefreshAudioUI();
        }
        public void SetSoundVolume(float _volume) {
            AudioPlayer.SetSoundVolume(_volume);
            RefreshAudioUI();
        }
        public void SetVoiceVolume(float _volume) {
            AudioPlayer.SetVoiceVolume(_volume);
            RefreshAudioUI();
        }
        void RefreshAudioUI() {
            MusicMuteImg.gameObject.SetActive(MusicSlider.value == 0);
            SoundMuteImg.gameObject.SetActive(SoundSlider.value == 0);
            VoiceMuteImg.gameObject.SetActive(VoiceSlider.value == 0);
            MusicValueText.text = TextManager.FloatToPercentStr(AudioPlayer.MusicVolumeRatio, 0);
            SoundValueText.text = TextManager.FloatToPercentStr(AudioPlayer.SoundVolumeRatio, 0);
            VoiceValueText.text = TextManager.FloatToPercentStr(AudioPlayer.VoiceVolumeRatio, 0);
        }
        void RefreshThridpartLinkUI() {
            bool linkedFB = FirebaseManager.IsLinkingThrdPart(ThirdPartLink.Facebook);
            FB_BlackCover.SetActive(!linkedFB);
            FB_Glow.SetActive(linkedFB);
            if (linkedFB)
                FB_Text.text = StringData.GetUIString("Linked");
            else
                FB_Text.text = StringData.GetUIString("UnLinked");

            bool linkedGoogle = FirebaseManager.IsLinkingThrdPart(ThirdPartLink.Google);
            Google_BlackCover.SetActive(!linkedGoogle);
            Google_Glow.SetActive(linkedGoogle);
            if (linkedGoogle)
                Google_Text.text = StringData.GetUIString("Linked");
            else
                Google_Text.text = StringData.GetUIString("UnLinked");
            if (Application.platform != RuntimePlatform.IPhonePlayer) {
                AppleGO.SetActive(false);
            } else {
                AppleGO.SetActive(true);
                bool linkedApple = FirebaseManager.IsLinkingThrdPart(ThirdPartLink.Apple);
                Apple_BlackCover.SetActive(!linkedApple);
                Apple_Glow.SetActive(linkedApple);
                if (linkedApple)
                    Apple_Text.text = StringData.GetUIString("Linked");
                else
                    Apple_Text.text = StringData.GetUIString("UnLinked");
            }

        }
        public void OnCopyAccountClick() {
            if (CDChecker.DoneCD("CopyAccount", 2)) {
                GUIUtility.systemCopyBuffer = GamePlayer.Instance.Data.UID;
                PopupUI.ShowPopupEvent(StringData.GetUIString("Setting_ACCopied") + "\n" + GamePlayer.Instance.Data.UID);
            }
        }

        public void OnCloseClick() {
            GamePlayer.Instance.SaveSettingToLoco();
            SetActive(false);
        }
        public void OnBackToStartClick() {
            OnCloseClick();
            PopupUI.InitSceneTransitionProgress();
            PopupUI.CallTransition(MyScene.StartScene);
        }

        public void OnPostProcessingChange() {
            GamePlayer.Instance.SetPostProcessing(PostProcessingToggle.isOn);
            PostProcessingManager.Instance.RefreshSetting();//更新後製效果
        }
        public void OnVibrationChange() {
            GamePlayer.Instance.SetVibration(VibrationToggle.isOn);
            if (VibrationToggle.isOn)
                Vibrator.Vibrate(GameSettingData.GetInt(GameSetting.VibrationOnVibrationMilliSecs));
        }

        public void OnLanguageSelectClick() {
            LanguageSelectGO.SetActive(true);
        }
        public void OnLanguageSelectConfirmClick() {
            GamePlayer.Instance.SetLanguage(CurLanguage);
            LanguageSelectGO.SetActive(false);
        }
        public void OnLanguageSelectCancelClick() {
            LanguageSelectGO.SetActive(false);
        }
        public void OnTWBtnClick() {
            CurLanguage = Language.TW;
            RefreshSelectLanguageUI();
        }
        public void OnCHBtnClick() {
            CurLanguage = Language.CH;
            RefreshSelectLanguageUI();
        }
        void RefreshSelectLanguageUI() {
            if (CurLanguage == Language.TW) {
                TWGO_On.SetActive(true);
                TWGO_Off.SetActive(false);
                CHGO_On.SetActive(false);
                CHGO_Off.SetActive(true);
            } else if (CurLanguage == Language.CH) {
                TWGO_On.SetActive(false);
                TWGO_Off.SetActive(true);
                CHGO_On.SetActive(true);
                CHGO_Off.SetActive(false);
            }
        }

        /// <summary>
        /// 顯示規章
        /// </summary>
        public void OnShowRule() {
            WebViewManager.Inst.ShowWebview(BackendURLType.SpecificationURL);
        }

        /// <summary>
        /// 回報問題
        /// </summary>
        public void OnShowReport() {
            string version = Application.version;
            WebViewManager.Inst.ShowWebview(BackendURLType.CustomerServiceURL, version, GamePlayer.Instance.Data.UID);
        }

        public void OnTutorialClick() {
            WebViewManager.Inst.ShowWebview(BackendURLType.RuleInfoURL);
        }

        /// <summary>
        /// 三方平台綁定按鈕按下
        /// </summary>
        public void OnThirdPartLinkClick(string _authentication) {

            ThirdPartLink thirdPartType;
            if (MyEnum.TryParseEnum(_authentication, out thirdPartType))
                thirdPartType = MyEnum.ParseEnum<ThirdPartLink>(_authentication);
            else {
                WriteLog.LogError("SettingUI按鈕傳入錯誤的綁定平台文字");
                return;
            }


            switch (thirdPartType) {
                case ThirdPartLink.Facebook:
                    if (!FirebaseManager.IsLinkingThrdPart(ThirdPartLink.Facebook)) {//還沒綁定就進行綁定
                        PopupUI.ShowLoading(StringData.GetUIString("Loading"));
                        FirebaseManager.LinkWithFacebook(result => {
                            RefreshThridpartLinkUI();//刷新UI
                            PopupUI.HideLoading();
                            if (result) {
                                PopupUI.ShowClickCancel(StringData.GetUIString("LinkSuccess"), null);
                            } else {//錯誤訊息的處理給各自平台的Function處理
                            }
                        });
                    } else {//已經綁定就跑解除綁定
                        PopupUI.ShowConfirmCancel(StringData.GetUIString("UnLinkCheck"), GameSettingData.GetInt(GameSetting.LogoutCowndownSecs), () => {
                            PopupUI.ShowLoading(StringData.GetUIString("Loading"));
                            FirebaseManager.UnlinkFacebook(result => {
                                PopupUI.HideLoading();
                                RefreshThridpartLinkUI();//刷新UI
                                if (result) {
                                    PopupUI.ShowClickCancel(StringData.GetUIString("UnLinkSuccess"), null);
                                } else {//錯誤訊息的處理給各自平台的Function處理
                                }
                            });
                        }, null);
                    }
                    break;
                case ThirdPartLink.Apple:
                    if (!FirebaseManager.IsLinkingThrdPart(ThirdPartLink.Apple)) {//還沒綁定就進行綁定
                        PopupUI.ShowLoading(StringData.GetUIString("Loading"));
                        FirebaseManager.LinkWithApple(result => {
                            RefreshThridpartLinkUI();//刷新UI
                            PopupUI.HideLoading();
                            if (result) {
                                PopupUI.ShowClickCancel(StringData.GetUIString("LinkSuccess"), null);
                            } else {//錯誤訊息的處理給各自平台的Function處理
                            }
                        });
                    } else {//已經綁定就跑解除綁定
                        PopupUI.ShowConfirmCancel(StringData.GetUIString("UnLinkCheck"), GameSettingData.GetInt(GameSetting.LogoutCowndownSecs), () => {
                            PopupUI.ShowLoading(StringData.GetUIString("Loading"));
                            FirebaseManager.UnlinkApple(result => {
                                RefreshThridpartLinkUI();//刷新UI
                                PopupUI.HideLoading();
                                if (result) {
                                    PopupUI.ShowClickCancel(StringData.GetUIString("UnLinkSuccess"), null);
                                } else {//錯誤訊息的處理給各自平台的Function處理
                                }
                            });
                        }, null);
                    }
                    break;
                case ThirdPartLink.Google:
                    if (Application.isEditor) {
                        PopupUI.ShowClickCancel("Editor不能使用Google登入", null);
                        return;
                    }
                    if (!FirebaseManager.IsLinkingThrdPart(ThirdPartLink.Google)) {//還沒綁定就進行綁定
                        PopupUI.ShowLoading(StringData.GetUIString("Loading"));
                        FirebaseManager.LinkWithGoogle(result => {
                            RefreshThridpartLinkUI();//刷新UI
                            PopupUI.HideLoading();
                            if (result) {
                                PopupUI.ShowClickCancel(StringData.GetUIString("LinkSuccess"), null);
                            } else {//錯誤訊息的處理給各自平台的Function處理
                            }
                        });
                    } else {//已經綁定就跑解除綁定
                        PopupUI.ShowConfirmCancel(StringData.GetUIString("UnLinkCheck"), GameSettingData.GetInt(GameSetting.LogoutCowndownSecs), () => {
                            PopupUI.ShowLoading(StringData.GetUIString("Loading"));
                            FirebaseManager.UnlinkGoogle(result => {
                                RefreshThridpartLinkUI();//刷新UI
                                PopupUI.HideLoading();
                                if (result) {
                                    PopupUI.ShowClickCancel(StringData.GetUIString("UnLinkSuccess"), null);
                                } else {//錯誤訊息的處理給各自平台的Function處理
                                }
                            });
                        }, null);

                    }
                    break;

            }
        }

    }
}