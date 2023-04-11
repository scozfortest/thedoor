using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;


namespace Scoz.Func {
    public class PopupUI_Local : MonoBehaviour {

        public static bool IsInit { get; private set; }
        static PopupUI_Local Instance;
        Canvas MyCanvas;

        //[HeaderAttribute("==============基本設定==============")]

        public static PopupUI_Local CreateNewInstance() {
            if (Instance != null) {
                //DebugLogger.Log("GameDictionary之前已經被建立了");
            } else {
                GameObject prefab = Resources.Load<GameObject>("Prefabs/Common/PopupUI_Local");
                GameObject go = Instantiate(prefab);
                go.name = "PopupUI_Local";
                Instance = go.GetComponent<PopupUI_Local>();
                Instance.Init();
            }
            return Instance;
        }
        void Start() {
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }

        void OnDestroy() {
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }
        private void Update() {
            if (ConfirmCancel_ConfirmBtnTimer != null) ConfirmCancel_ConfirmBtnTimer.RunTimer();
        }

        void OnLevelFinishedLoading(Scene _scene, LoadSceneMode _mode) {
            GameManager.Instance.AddCamStack(GetComponent<Camera>());//將自己的camera加入到目前場景上的MainCameraStack中
        }
        public void Init() {
            Instance = this;
            GameManager.Instance.AddCamStack(GetComponent<Camera>());//將自己的camera加入到目前場景上的MainCameraStack中
            MyCanvas = GetComponent<Canvas>();
            InitLoading();
            InitClickCamcel();
            InitConfirmCancel();
            DontDestroyOnLoad(gameObject);
            DebugLogger.Log("初始化PopupUI_Local");
        }



        [HeaderAttribute("==============讀取彈窗==============")]
        [SerializeField]
        GameObject LoadingGo = null;
        [SerializeField]
        Text LoadingText = null;
        static float LoadingMaxTime = 30;
        static int LoadingCoroutineID;

        void InitLoading() {
            LoadingGo.SetActive(false);
        }
        public static void ShowLoading(string _text, float _maxLoadingTime = 0) {
            if (!Instance)
                return;
            if (_maxLoadingTime == 0)
                _maxLoadingTime = LoadingMaxTime;
            Instance.LoadingGo.SetActive(true);
            Instance.LoadingText.text = _text;


            CoroutineJob.Instance.StopCoroutine(LoadingCoroutineID);
            if (_maxLoadingTime > 0) {
                LoadingCoroutineID = CoroutineJob.Instance.StartNewAction(() => {
                    //ShowClickCancel(StringData.GetUIString("ConnectTimeout"), null);
                    HideLoading();
                }, _maxLoadingTime);
            }
        }
        public static void HideLoading() {
            if (!Instance)
                return;
            CoroutineJob.Instance.StopCoroutine(LoadingCoroutineID);
            Instance.LoadingGo.SetActive(false);
        }

        [HeaderAttribute("==============點擊關閉彈窗==============")]
        [SerializeField]
        GameObject ClickCancelGo = null;
        [SerializeField]
        Text ClickCancelText = null;
        Action ClickCancelAction = null;
        Action<object> ClickCancelActionWithParam = null;
        object ClickCancelParam;

        void InitClickCamcel() {
            ClickCancelGo.SetActive(false);
        }

        public static void ShowClickCancel(string _text, Action _clickCancelAction) {
            if (!Instance)
                return;
            Instance.ClickCancelGo.SetActive(true);
            Instance.ClickCancelText.text = _text;
            Instance.ClickCancelAction = _clickCancelAction;
        }
        public static void ShowClickCancel(string _text, Action<object> _clickCancelAction, object _param) {
            if (!Instance)
                return;
            Instance.ClickCancelGo.SetActive(true);
            Instance.ClickCancelText.text = _text;
            Instance.ClickCancelActionWithParam = _clickCancelAction;
            Instance.ClickCancelParam = _param;
        }
        public void OnClickCancelClick() {
            if (!Instance)
                return;
            Instance.ClickCancelGo.SetActive(false);
            ClickCancelAction?.Invoke();
            ClickCancelActionWithParam?.Invoke(ClickCancelParam);
        }


        [HeaderAttribute("==============確認/取消彈窗==============")]
        [SerializeField] GameObject ConfirmCancelGo = null;
        [SerializeField] Text ConfirmCancelText = null;
        [SerializeField] Text ConfirmCancel_ConfirmBtnText = null;
        [SerializeField] Text ConfirmCancel_CancelBtnText = null;
        [SerializeField] Button ConfirmCancel_ConfirmBtn;
        Action ConfirmCancelAction_Click = null;
        Action ConfirmCancelAction_Cancel = null;
        Action<object> ConfirmCancelAction_Click_WithParam = null;
        Action<object> ConfirmCancelAction_Cancel_WithParam = null;
        object ConfirmCancel_ConfirmParam;
        object ConfirmCancel_CancelParam;
        MyTimer ConfirmCancel_ConfirmBtnTimer;
        int ConfirmCanClickCoundownSecs;
        void InitConfirmCancel() {
            ConfirmCancelGo.SetActive(false);
        }
        /// <summary>
        /// 顯示確認取消視窗
        /// </summary>
        public static void ShowConfirmCancel(string _text, Action _confirmAction, Action _cancelAction) {
            if (!Instance)
                return;
            Instance.ConfirmCancelGo.SetActive(true);
            Instance.ConfirmCancelText.text = _text;
            Instance.ConfirmCancelAction_Click = _confirmAction;
            Instance.ConfirmCancelAction_Cancel = _cancelAction;
            Instance.ConfirmCancelAction_Click_WithParam = null;
            Instance.ConfirmCancelAction_Cancel_WithParam = null;
            Instance.ConfirmCancel_ConfirmBtnText.text = StringData.GetUIString("Confirm");
            Instance.ConfirmCancel_CancelBtnText.text = StringData.GetUIString("Cancel");
            Instance.ConfirmCancel_ConfirmBtnTimer = null;
            Instance.ConfirmCancel_ConfirmBtn.interactable = true;
        }
        /// <summary>
        /// 顯示確認取消視窗 且 確認按鈕有倒數 倒數完才能點確認
        /// </summary>
        public static void ShowConfirmCancel(string _text, int _confirmCanClickCoundownSecs, Action _confirmAction, Action _cancelAction) {
            if (!Instance)
                return;
            if (_confirmCanClickCoundownSecs > 0) {
                Instance.ConfirmCanClickCoundownSecs = _confirmCanClickCoundownSecs;
                Instance.ConfirmCancel_ConfirmBtn.interactable = false;
                SetConfirmBtnText();
                Instance.ConfirmCancel_ConfirmBtnTimer = new MyTimer(1, () => {
                    Instance.ConfirmCanClickCoundownSecs--;
                    SetConfirmBtnText();
                }, true, true);
            } else {
                Instance.ConfirmCancel_ConfirmBtnTimer = null;
                Instance.ConfirmCancel_ConfirmBtnText.text = StringData.GetUIString("Confirm");
                Instance.ConfirmCancel_ConfirmBtn.interactable = true;
            }

            Instance.ConfirmCancelGo.SetActive(true);
            Instance.ConfirmCancelText.text = _text;
            Instance.ConfirmCancelAction_Click = _confirmAction;
            Instance.ConfirmCancelAction_Cancel = _cancelAction;
            Instance.ConfirmCancelAction_Click_WithParam = null;
            Instance.ConfirmCancelAction_Cancel_WithParam = null;
            Instance.ConfirmCancel_CancelBtnText.text = StringData.GetUIString("Cancel");
        }
        static void SetConfirmBtnText() {
            if (Instance.ConfirmCanClickCoundownSecs > 0) {
                Instance.ConfirmCancel_ConfirmBtn.interactable = false;
                Instance.ConfirmCancel_ConfirmBtnText.text = string.Format(StringData.GetUIString("CowndownSec"), Instance.ConfirmCanClickCoundownSecs.ToString());
            } else {
                Instance.ConfirmCancel_ConfirmBtnTimer = null;
                Instance.ConfirmCancel_ConfirmBtn.interactable = true;
                Instance.ConfirmCancel_ConfirmBtnText.text = StringData.GetUIString("Confirm");
            }
        }
        public static void ShowConfirmCancel(string _text, string _confirmText, string _cancelText, Action _confirmAction, Action _cancelAction) {
            if (!Instance)
                return;


            Instance.ConfirmCancelGo.SetActive(true);
            Instance.ConfirmCancelText.text = _text;
            Instance.ConfirmCancelAction_Click = _confirmAction;
            Instance.ConfirmCancelAction_Cancel = _cancelAction;
            Instance.ConfirmCancelAction_Click_WithParam = null;
            Instance.ConfirmCancelAction_Cancel_WithParam = null;
            Instance.ConfirmCancel_ConfirmBtnText.text = _confirmText;
            Instance.ConfirmCancel_CancelBtnText.text = _cancelText;
            Instance.ConfirmCancel_ConfirmBtnTimer = null;
            Instance.ConfirmCancel_ConfirmBtn.interactable = true;
        }



        public static void ShowConfirmCancel(string _text, Action<object> _confirmAction, object _confirmParam, Action<object> _cancelAction, object _cancelParam) {
            if (!Instance)
                return;
            Instance.ConfirmCancelGo.SetActive(true);
            Instance.ConfirmCancelText.text = _text;
            Instance.ConfirmCancelAction_Click = null;
            Instance.ConfirmCancelAction_Cancel = null;
            Instance.ConfirmCancelAction_Click_WithParam = _confirmAction;
            Instance.ConfirmCancelAction_Cancel_WithParam = _cancelAction;
            Instance.ConfirmCancel_ConfirmParam = _confirmParam;
            Instance.ConfirmCancel_CancelParam = _cancelParam;
            Instance.ConfirmCancel_ConfirmBtnText.text = StringData.GetUIString("Confirm");
            Instance.ConfirmCancel_CancelBtnText.text = StringData.GetUIString("Cancel");
            Instance.ConfirmCancel_ConfirmBtnTimer = null;
            Instance.ConfirmCancel_ConfirmBtn.interactable = true;
        }

        public void OnConfirmCancel_ConfirmClick() {
            if (!Instance)
                return;
            Instance.ConfirmCancelGo.SetActive(false);
            ConfirmCancelAction_Click?.Invoke();
            ConfirmCancelAction_Click_WithParam?.Invoke(ConfirmCancel_ConfirmParam);
        }

        public void OnConfirmCancel_CancelClick() {
            if (!Instance)
                return;
            Instance.ConfirmCancelGo.SetActive(false);
            ConfirmCancelAction_Cancel?.Invoke();
            ConfirmCancelAction_Cancel_WithParam?.Invoke(ConfirmCancel_CancelParam);
        }



    }
}
