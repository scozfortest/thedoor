using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Scoz.Func {
    public class PopupEventItem : MonoBehaviour, IItem {

        [SerializeField]
        Text TitleText = null;
        [SerializeField]
        Animator MyAni = null;

        object[] ActionParameters;
        Action<object[]> ClickAction;


        public static int WaitToShowCount = 0;
        [HideInInspector]
        public bool IsActive { get; set; }

        public void Init(string _title) {
            ClickAction = null;
            ActionParameters = null;
            InitSet(_title);
        }
        public void Init(string _title, Action<object[]> _action, params object[] _parameters) {
            ClickAction = _action;
            ActionParameters = _parameters;
            InitSet(_title);
        }
        void InitSet(string _title) {

            if (_title != "") {
                TitleText.gameObject.SetActive(true);
                TitleText.text = _title;
            } else
                TitleText.gameObject.SetActive(false);
            IsActive = true;
            WaitToShowCount++;
            if (WaitToShowCount < 2)
                MyAni.Play("play");
            else {
                CoroutineJob.Instance.StartNewAction(Show, GameSettingData.GetFloat(GameSetting.EventPopupMinimumInterval) * (WaitToShowCount - 1));
            }
        }

        void Show() {
            MyAni.Play("play");
        }
        public void IsEndPlay() {
            IsActive = false;
            WaitToShowCount--;
        }
        public void OnClick() {
            MyAni.Play("click");
            ClickAction?.Invoke(ActionParameters);
        }

    }
}