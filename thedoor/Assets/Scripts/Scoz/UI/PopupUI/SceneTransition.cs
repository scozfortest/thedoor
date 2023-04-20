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

    public class SceneTransition : MonoBehaviour {
        //進遊戲不先初始化，等到要用時才初始化UI
        [HeaderAttribute("==============轉場UI==============")]
        [SerializeField] Animator TransitionAni = null;
        [SerializeField] Image SceneTransitionProgress = null;
        [SerializeField] Image SceneTransitionImg = null;
        [SerializeField] Image SceneTransitionBGImg2 = null;
        [SerializeField] Text SceneTransitionText = null;
        [SerializeField] Text SceneTransitionPlayerUIDText;
        [SerializeField] float WaitSecAfterFinishProgressPercent = 0.8f;
        public static MyScene PreviousScene = MyScene.StartScene;
        MyScene GoScene;
        AsyncOperationHandle TransitionBGHandle;


        float WaitSecAfterFinish = 0;//讀取完等待幾秒後才離開轉場 因為進到Lobby會載入UI到記憶中 這段時間會卡卡的 所以可以設定這個等待秒數讓讀取介面多停久一點再離開讀取介面
        LoadingProgress MyLoadingProgress;//讀取進度，讀取完會執行傳入的Callback Action
        Action FinishAC;

        public void InitTransition() {
            MyLoadingProgress = new LoadingProgress(CallEndTransition);
            TransitionAni.gameObject.SetActive(true);
            SceneTransitionPlayerUIDText.text = FirebaseManager.MyUser.UserId;
        }
        public void InitSceneTransitionProgress(float _waitSecAfterFinish = 0, params string[] _keys) {
            WaitSecAfterFinish = _waitSecAfterFinish;
            if (Application.isEditor)
                WaitSecAfterFinish = 0;
            InitSceneTransitionProgress(_keys);
        }
        public void InitSceneTransitionProgress(params string[] _keys) {
            MyLoadingProgress.ResetProgress();
            MyLoadingProgress.AddLoadingProgress("SceneTransitionScene");
            MyLoadingProgress.AddLoadingProgress(_keys);
        }

        public void FinishTransitionProgress(string _key) {
            MyLoadingProgress.FinishProgress(_key);
        }

        public void CallTransition(MyScene _scene, Action _ac) {
            FinishAC = _ac;
            SceneTransitionProgress.fillAmount = 0;
            TransitionAni.SetTrigger("Play");
            SceneTransitionData data = SceneTransitionData.GetRandomData();
            if (data != null) {
                SceneTransitionText.text = data.Description;
                if (!string.IsNullOrEmpty(data.RefPic)) {
                    SceneTransitionImg.gameObject.SetActive(true);
                    AddressablesLoader.GetSprite(data.RefPic, (sprite, handle) => {
                        if (TransitionBGHandle.IsValid())
                            Addressables.Release(TransitionBGHandle);
                        TransitionBGHandle = handle;
                        SceneTransitionImg.sprite = sprite;
                    });
                } else
                    SceneTransitionImg.gameObject.SetActive(false);
            }
            GoScene = _scene;
        }
        public void OnSceneTransition() {
            StartCoroutine(LoadAsyncScene(GoScene));
        }

        IEnumerator LoadAsyncScene(MyScene _scene) {
            string tmpScene = SceneManager.GetActiveScene().name;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_scene.ToString());

            while (!asyncLoad.isDone) {
                float progress = 0;
                if (WaitSecAfterFinish > 0) {
                    progress = asyncLoad.progress * (1 - WaitSecAfterFinishProgressPercent);
                } else
                    progress = asyncLoad.progress;
                SceneTransitionBGImg2.color = new Color(SceneTransitionBGImg2.color.r, SceneTransitionBGImg2.color.g, SceneTransitionBGImg2.color.b, progress);
                SceneTransitionProgress.fillAmount = progress;
                yield return null;
            }
            if (WaitSecAfterFinish > 0) {
                float progress = 0;
                float tmpWaitSecAfterFinish = WaitSecAfterFinish;
                while (tmpWaitSecAfterFinish > 0) {
                    tmpWaitSecAfterFinish -= 0.1f;
                    progress = (1 - WaitSecAfterFinishProgressPercent) + WaitSecAfterFinishProgressPercent * (1 - tmpWaitSecAfterFinish / WaitSecAfterFinish);
                    SceneTransitionBGImg2.color = new Color(SceneTransitionBGImg2.color.r, SceneTransitionBGImg2.color.g, SceneTransitionBGImg2.color.b, progress);
                    SceneTransitionProgress.fillAmount = progress;
                    yield return new WaitForSeconds(0.1f);
                }
            }


            PreviousScene = MyEnum.ParseEnum<MyScene>(tmpScene);//紀錄上一個場景的Scene
            FinishTransitionProgress("SceneTransitionScene");
        }
        public void CallEndTransition() {
            End();
        }
        void End() {
            if (TransitionBGHandle.IsValid())
                Addressables.Release(TransitionBGHandle);
            TransitionAni.SetTrigger("End");
            GameManager.UnloadUnusedAssets();//結束Transition就順便釋放記憶體
            FinishAC?.Invoke();
        }

    }
}