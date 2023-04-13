using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TheDoor.Main;
using System;

namespace Scoz.Func {
    public partial class PopupUI {
        //進遊戲不先初始化，等到要用時才初始化UI
        [HeaderAttribute("==============轉場UI==============")]
        //進遊戲不先初始化，等到要用時才初始化UI
        [SerializeField] AssetReference SceneTransitionAsset;
        [SerializeField] Transform SceneTransitionParent;
        SceneTransition MySceneTransition = null;
        Action OnSceneTransitionAssetLoadFinishedAC;//載完Asset後要執行的Action
        bool IsLoadingAsse = false;

        void InitSceneTransition() {
            if (IsLoadingAsse)
                return;
            IsLoadingAsse = true;
            PopupUI.ShowLoading(StringData.GetUIString("Loading"));
            //初始化UI
            AddressablesLoader.GetPrefabByRef(Instance.SceneTransitionAsset, (prefab, handle) => {
                IsLoadingAsse = false;
                PopupUI.HideLoading();
                GameObject go = Instantiate(prefab);
                go.transform.SetParent(Instance.SceneTransitionParent);
                go.transform.localPosition = prefab.transform.localPosition;
                go.transform.localScale = prefab.transform.localScale;
                //RectTransform rect = go.GetComponent<RectTransform>();
                //rect.offsetMin = Vector2.zero;//Left、Bottom
                //rect.offsetMax = Vector2.zero;//Right、Top
                go.transform.SetAsLastSibling();
                Instance.MySceneTransition = go.GetComponent<SceneTransition>();
                Instance.MySceneTransition.gameObject.SetActive(true);
                Instance.MySceneTransition.InitTransition();
                Instance.OnSceneTransitionAssetLoadFinishedAC?.Invoke();
            }, () => { WriteLog.LogError("載入SceneTransitionAsset失敗"); });
        }
        public static void InitSceneTransitionProgress(params string[] _keys) {
            if (Instance == null)
                return;
            InitSceneTransitionProgress(0, _keys);
        }
        public static void InitSceneTransitionProgress(float _waitSecAfterFinish, params string[] _keys) {
            if (Instance == null)
                return;

            //判斷是否已經載入過此UI，若還沒載過就跳讀取中並開始載入
            if (Instance.MySceneTransition != null) {
                Instance.MySceneTransition.InitSceneTransitionProgress(_waitSecAfterFinish, _keys);
            } else {
                Instance.OnSceneTransitionAssetLoadFinishedAC += () => { Instance.MySceneTransition.InitSceneTransitionProgress(_waitSecAfterFinish, _keys); };
                Instance.InitSceneTransition();
            }
        }

        public static void FinishTransitionProgress(string _key) {
            if (Instance == null)
                return;

            //判斷是否已經載入過此UI，若還沒載過就跳讀取中並開始載入
            if (Instance.MySceneTransition != null) {
                Instance.MySceneTransition.FinishTransitionProgress(_key);
            } else {
                Instance.OnSceneTransitionAssetLoadFinishedAC += () => { Instance.MySceneTransition.FinishTransitionProgress(_key); };
                Instance.InitSceneTransition();
            }

        }

        public static void CallTransition(MyScene _scene) {
            if (Instance == null)
                return;

            //判斷是否已經載入過此UI，若還沒載過就跳讀取中並開始載入
            if (Instance.MySceneTransition != null) {
                Instance.MySceneTransition.CallTransition(_scene);
            } else {
                Instance.OnSceneTransitionAssetLoadFinishedAC += () => { Instance.MySceneTransition.CallTransition(_scene); };
                Instance.InitSceneTransition();
            }

        }
    }
}
