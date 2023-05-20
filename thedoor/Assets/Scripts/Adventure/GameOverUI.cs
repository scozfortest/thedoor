using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Scoz.Func;
using System;
using TMPro;

namespace TheDoor.Main {

    public class GameOverUI : BaseUI {

        [SerializeField] Animator TransitionAni = null;
        [SerializeField] Image TransitionImg = null;
        [SerializeField] TextMeshProUGUI TransitionText = null;


        float WaitMinSec = 0;
        LoadingProgress MyLoadingProgress;//讀取進度，讀取完會執行傳入的Callback Action
        Action FinishAC;

        public static GameOverUI Instance { get; private set; }


        public override void Init() {
            base.Init();
            Instance = this;
            MyLoadingProgress = new LoadingProgress(End);
        }
        void AddTransitionProgress(string _key) {
            MyLoadingProgress.FinishProgress(_key);
        }

        public void CallTransition(Sprite _sprite, string _description, float _waitMinSec, Action _ac, params string[] _addLoadingProgress) {
            SetActive(true);
            DoorNodeUI.Instance?.SetActive(false);
            FinishAC = _ac;
            WaitMinSec = _waitMinSec;
            TransitionAni.SetTrigger("Play");
            MyLoadingProgress.ResetProgress();
            MyLoadingProgress.AddLoadingProgress("WaitMinSec");
            MyLoadingProgress.AddLoadingProgress("AniEnd");
            MyLoadingProgress.AddLoadingProgress("OnClick");
            MyLoadingProgress.AddLoadingProgress(_addLoadingProgress);
            TransitionText.text = _description;
            TransitionImg.gameObject.SetActive(true);
            TransitionImg.sprite = _sprite;
            TransitionImg.SetNativeSize();
            CoroutineJob.Instance.StartNewAction(() => {
                AddTransitionProgress("WaitMinSec");
            }, WaitMinSec);
        }

        public void OnStartAniEnd() {
            AddTransitionProgress("AniEnd");
        }
        void End() {
            TransitionAni.SetTrigger("End");
            GameManager.UnloadUnusedAssets();//結束Transition就順便釋放記憶體
            FinishAC?.Invoke();
        }
        public void OnClick() {
            AddTransitionProgress("OnClick");
        }

    }
}