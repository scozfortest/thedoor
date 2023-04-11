using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

namespace TheDoor.Main {
    public class TriggerEventUI : BaseUI {
        [SerializeField] Image PicImg;
        [SerializeField] Text TitleText;
        [SerializeField] Text ContentText;
        [SerializeField] Text BtnText;

        Action ConfirmAC;
        Action CloseAC;
        void Start() {
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }
        void OnLevelFinishedLoading(Scene _scene, LoadSceneMode _mode) {
            if (_scene.name != MyScene.LobbyScene.ToString()) {//如果切換至不是大廳都會關事件UI
                SetActive(false);
            }
        }
        public void ShowUI(TriggerEvent _event, string _param, Action _confirmAC, Action _closeAC) {
            ConfirmAC = _confirmAC;
            CloseAC = _closeAC;
            string titleStrKey = "TriggerEventUI_" + _event.ToString() + "_Title";
            string contentStrKey = "TriggerEventUI_" + _event.ToString() + "_Content";
            string btnStrKey = "TriggerEventUI_" + _event.ToString() + "_Btn";

            switch (_event) {
                case TriggerEvent.Request_GoLinkUI:
                    TitleText.text = StringData.GetUIString(titleStrKey);
                    ContentText.text = StringData.GetUIString(contentStrKey);
                    BtnText.text = StringData.GetUIString(btnStrKey);
                    titleStrKey = "TriggerEventUI_" + _event.ToString() + "_" + _param + "_Title";
                    contentStrKey = "TriggerEventUI_" + _event.ToString() + "_" + _param + "_Content";
                    btnStrKey = "TriggerEventUI_" + _event.ToString() + "_" + _param + "_Btn";
                    AddressablesLoader.GetSpriteAtlas("TriggerEventUI", atlas => {
                        PicImg.sprite = atlas.GetSprite(_param);
                    });
                    break;
                default:
                    AddressablesLoader.GetSpriteAtlas("TriggerEventUI", atlas => {
                        PicImg.sprite = atlas.GetSprite(_event.ToString());
                    });
                    break;
            }
            TitleText.text = StringData.GetUIString(titleStrKey);
            ContentText.text = StringData.GetUIString(contentStrKey);
            BtnText.text = StringData.GetUIString(btnStrKey);
            SetActive(true);
        }
        public void OnCloseClick() {
            SetActive(false);
            CloseAC?.Invoke();
        }
        public void OnConfrimClick() {
            SetActive(false);
            ConfirmAC?.Invoke();
        }
    }
}