using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TheDoor.Main;
using System;
using TMPro;

namespace Scoz.Func {

    public class UITransition : MonoBehaviour {
        //進遊戲不先初始化，等到要用時才初始化UI
        [HeaderAttribute("==============轉場UI==============")]
        [SerializeField] Animator TransitionAni = null;
        [SerializeField] Image TransitionImg = null;
        [SerializeField] TextMeshProUGUI TransitionText = null;


        float WaitMinSec = 0;
        LoadingProgress MyLoadingProgress;//讀取進度，讀取完會執行傳入的Callback Action
        Action FinishAC;


        public void InitTransition() {
            MyLoadingProgress = new LoadingProgress(End);
            TransitionAni.gameObject.SetActive(true);
        }
        public void SetTransitionProgress(params string[] _keys) {
            MyLoadingProgress.ResetProgress();
            MyLoadingProgress.AddLoadingProgress(_keys);
        }

        public void FinishTransitionProgress(string _key) {
            MyLoadingProgress.FinishProgress(_key);
        }

        public void CallTransition(Sprite _sprite, string _description, float _waitMinSec, Action _ac = null) {
            FinishAC = _ac;
            WaitMinSec = _waitMinSec;
            TransitionAni.SetTrigger("Play");
            MyLoadingProgress.AddLoadingProgress("WaitMinSec");
            MyLoadingProgress.AddLoadingProgress("AniEnd");
            TransitionText.text = _description;
            TransitionImg.gameObject.SetActive(true);
            TransitionImg.sprite = _sprite;
            TransitionImg.SetNativeSize();

        }
        public void OnTransition() {
            CoroutineJob.Instance.StartNewAction(() => {
                FinishTransitionProgress("TransitionScene");
            }, WaitMinSec);
        }

        public void CallEndTransition() {
            FinishTransitionProgress("AniEnd");
        }
        void End() {
            TransitionAni.SetTrigger("End");
            GameManager.UnloadUnusedAssets();//結束Transition就順便釋放記憶體
            FinishAC?.Invoke();
        }

    }
}