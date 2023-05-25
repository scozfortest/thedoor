using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;


namespace Scoz.Func {
    public class MyVideoPlayer : MonoBehaviour {
        public static MyVideoPlayer Instance;
        [SerializeField] VideoPlayer TheVideoPlayer = null;
        [SerializeField] Button SkipBtn;
        public static RenderTexture rt;
        static bool IsPlayingVideo = false;

        public delegate void VideoEndFunc(VideoPlayer _vp);

        public VideoEndFunc MyClipEndFunc = null;
        public VideoEndFunc MyVideoEndFunc = null;
        public VideoEndFunc MyVideoRClickFunc = null;
        public VideoEndFunc MyVideoLClickFunc = null;
        [SerializeField]
        RawImage VideoImage = null;


        public static MyVideoPlayer CreateNewVideoPlayer() {
            if (Instance != null) {
                //DebugLogger.Log("VideoPlayer之前已經被建立了");
            } else {
                GameObject prefab = Resources.Load<GameObject>("Prefabs/Common/MyVideoPlayer");
                GameObject go = Instantiate(prefab);
                go.name = "MyVideoPlayer";
                Instance = go.GetComponent<MyVideoPlayer>();
                Instance.Init();
            }
            return Instance;
        }

        void Init() {
            TheVideoPlayer.gameObject.SetActive(true);
            GameManager.Instance.AddCamStack(GetComponent<Camera>());//將自己的camera加入到目前場景上的MainCameraStack中
            DontDestroyOnLoad(gameObject);
            InitVideo();
            gameObject.SetActive(false);
        }
        void OnLevelFinishedLoading(Scene _scene, LoadSceneMode _mode) {
            Debug.LogError("aaaaaaaaaaaa");
            GameManager.Instance.AddCamStack(GetComponent<Camera>());//將自己的camera加入到目前場景上的MainCameraStack中
        }
        private void OnDestroy() {
            Instance = null;
        }
        private void Update() {
            OnVideoInputFunc();
        }
        void InitVideo() {
            TheVideoPlayer.loopPointReached += OnClipEnd;
            MyClipEndFunc = null;
            MyVideoEndFunc = null;
            MyVideoRClickFunc = null;
            MyVideoLClickFunc = null;
        }
        public void PlayVideo(string _url, bool _showSkip) {
            gameObject.SetActive(true);
            if (SkipBtn != null) SkipBtn.gameObject.SetActive(_showSkip);
            TheVideoPlayer.url = _url;
            TheVideoPlayer.Play();
        }
        public void PlayVideo(VideoClip _vp, bool _showSkip) {
            gameObject.SetActive(true);
            if (SkipBtn != null) SkipBtn.gameObject.SetActive(_showSkip);
            //DebugLogger.Log("Width=" + _vp.width);
            //DebugLogger.Log("Height=" + _vp.height);
            IsPlayingVideo = true;
            TheVideoPlayer.source = VideoSource.VideoClip;
            TheVideoPlayer.clip = _vp;
            rt = new RenderTexture((int)_vp.width, (int)_vp.height, 24);
            rt.Create();
            VideoImage.texture = rt;
            TheVideoPlayer.targetTexture = rt;
            //Myself.VideoImage.texture.width = (int)_vp.width;
            //Myself.VideoImage.texture.height = (int)_vp.height;
            TheVideoPlayer.isLooping = true;
            VideoImage.gameObject.SetActive(false);
            TheVideoPlayer.prepareCompleted += PrepareCompleted;
        }
        public void PlayVideo(string path, bool _showSkip, int width = 1920, int height = 1080) {
            gameObject.SetActive(true);
            if (SkipBtn != null) SkipBtn.gameObject.SetActive(_showSkip);
            //DebugLogger.Log("Width=" + _vp.width);
            //DebugLogger.Log("Height=" + _vp.height);
            IsPlayingVideo = true;
            TheVideoPlayer.source = VideoSource.Url;
            TheVideoPlayer.url = Application.streamingAssetsPath + path;
            rt = new RenderTexture(width, height, 24);
            rt.Create();
            VideoImage.texture = rt;
            TheVideoPlayer.targetTexture = rt;
            //Myself.VideoImage.texture.width = (int)_vp.width;
            //Myself.VideoImage.texture.height = (int)_vp.height;
            TheVideoPlayer.isLooping = true;
            VideoImage.gameObject.SetActive(false);
            TheVideoPlayer.prepareCompleted += PrepareCompleted;
        }
        void PrepareCompleted(VideoPlayer vp) {
            VideoImage.gameObject.SetActive(true);
            vp.Play();
        }
        public void EndVideo() {
            gameObject.SetActive(false);
            IsPlayingVideo = false;
            if (MyVideoEndFunc != null) {
                MyVideoEndFunc(TheVideoPlayer);
            }
        }

        public void OnSkipClick() {
            EndVideo();
        }
        public void AddOnClipEndFunc(VideoEndFunc _func) {
            MyClipEndFunc = _func;
        }
        public void AddOnVideoEndFunc(VideoEndFunc _func) {
            MyVideoEndFunc = _func;
        }
        public void AddOnVideoLeftClickFunc(VideoEndFunc _func) {
            MyVideoLClickFunc = _func;
        }
        public void AddOnVideoRightClickFunc(VideoEndFunc _func) {
            MyVideoRClickFunc = _func;
        }
        public void OnClipEnd(VideoPlayer _vp) {
            if (MyClipEndFunc != null)
                MyClipEndFunc(_vp);
        }
        void OnVideoInputFunc() {
            if (!IsPlayingVideo)
                return;
            if (Input.GetMouseButtonDown(0)) {
                if (MyVideoLClickFunc != null) {
                    MyVideoLClickFunc(TheVideoPlayer);
                }
            }

            if (Input.GetMouseButtonDown(1))
                if (MyVideoRClickFunc != null)
                    MyVideoRClickFunc(TheVideoPlayer);
        }
    }
}
