using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using TheDoor.Main;
using UnityEngine.AddressableAssets;

namespace Scoz.Func {
    public class PopupEventSpawner : ItemSpawner<PopupEventItem> { }
    public partial class PopupUI : MonoBehaviour {
        static PopupUI Instance;
        Canvas MyCanvas;



        //[HeaderAttribute("==============基本設定==============")]

        public void Init() {
            Instance = this;
            GameManager.Instance.AddCamStack(GetComponent<Camera>());//將自己的camera加入到目前場景上的MainCameraStack中
            DontDestroyOnLoad(gameObject);
            MyCanvas = GetComponent<Canvas>();

            InitGameInfo();
            InitTriggerEventUI();
            InitLoading();
            InitPopupEvent();
            InitClickCancel();
            InitConfirmCancel();
            InitInput();
            InitScreenEffect();
            InitRoleInfoUI();

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


        [HeaderAttribute("==============說明彈窗==============")]
        [SerializeField] GameObject GameInfo_GO = null;
        [SerializeField] Text GameInfo_Content = null;
        [SerializeField] ContentSizeFitter GameInfo_ContentSizeFitter = null;
        void InitGameInfo() {
            if (GameInfo_GO == null) return;
            GameInfo_GO.SetActive(false);
        }
        public static void ShowGameInfo(string _infoUIStringID) {
            if (Instance.GameInfo_GO == null) return;
            string content = StringData.GetUIString(_infoUIStringID);
            Instance.GameInfo_Content.text = content;
            CoroutineJob.Instance.StartNewAction(() => { Instance.GameInfo_ContentSizeFitter.Update(); }, 0.01f);
            Instance.GameInfo_GO.SetActive(true);
        }
        public void OnCloseInfoClick() {
            GameInfo_GO.SetActive(false);
        }

        [HeaderAttribute("==============觸發事件彈窗==============")]
        [SerializeField] TriggerEventUI MyTriggerEventUI = null;
        void InitTriggerEventUI() {
            if (MyTriggerEventUI == null) return;
            MyTriggerEventUI.SetActive(false);
        }
        public static void ShowTriggerEventUI(TriggerEvent _event, string _param, Action _confirmAC, Action _closeAC) {
            if (Instance.MyTriggerEventUI == null) return;
            Instance.MyTriggerEventUI.ShowUI(_event, _param, _confirmAC, _closeAC);
        }


        [HeaderAttribute("==============讀取彈窗==============")]
        [SerializeField] GameObject LoadingGo = null;
        [SerializeField] Text LoadingText = null;
        [SerializeField] Text LoadinPlayerUIDText;
        static float LoadingMaxTime = 30;
        static int LoadingCoroutineID;
        static string LoadingTimeOutStr;

        void InitLoading() {
            LoadingGo.SetActive(false);
            LoadingMaxTime = GameSettingData.GetFloat(GameSetting.LoadingMaxTime);
            LoadinPlayerUIDText.text = FirebaseManager.MyUser.UserId;
        }
        public static void ShowLoading(string _text, float _maxLoadingTime = 0, string _loadingTimeOutStr = "") {
            if (!Instance)
                return;
            if (_maxLoadingTime == 0)
                _maxLoadingTime = LoadingMaxTime;
            GameManager.UnloadUnusedAssets();//趁Loading時偷偷將null資源釋放
            Instance.LoadingGo.SetActive(true);
            Instance.LoadingText.text = _text;
            LoadingTimeOutStr = _loadingTimeOutStr;

            CoroutineJob.Instance.StopCoroutine(LoadingCoroutineID);
            if (_maxLoadingTime > 0) {
                LoadingCoroutineID = CoroutineJob.Instance.StartNewAction(() => {
                    HideLoading();
                    if (!string.IsNullOrEmpty(_loadingTimeOutStr))
                        ShowClickCancel(LoadingTimeOutStr, null);
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

        void InitClickCancel() {
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
            Instance.ConfirmCancel_ConfirmBtnText.text = StringData.GetUIString("Confirm");
            Instance.ConfirmCancel_CancelBtnText.text = StringData.GetUIString("Cancel");
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


        [HeaderAttribute("==============輸入彈窗==============")]
        [SerializeField]
        GameObject InputGo = null;
        [SerializeField]
        Text InputTitleText = null;
        [SerializeField]
        InputField MyInputField = null;
        Action<string> InputAction_Click_WithParam = null;
        Action InputAction_Cancel = null;
        void InitInput() {
            InputGo.SetActive(false);
        }


        public static void ShowInput(string _titleText, string _placeholder, string _text, Action<string> _confirmAction, Action _cancelAction) {
            if (!Instance)
                return;
            Instance.InputGo.SetActive(true);
            Instance.InputTitleText.text = _titleText;
            Instance.MyInputField.text = _text;
            Instance.MyInputField.placeholder.GetComponent<Text>().text = _placeholder;
            Instance.InputAction_Click_WithParam = _confirmAction;
            Instance.InputAction_Cancel = _cancelAction;
        }

        public void OnInput_ConfirmClick() {
            if (!Instance)
                return;
            Instance.InputGo.SetActive(false);
            InputAction_Click_WithParam?.Invoke(MyInputField.text);
        }

        public void OnInput_CancelClick() {
            if (!Instance)
                return;
            Instance.InputGo.SetActive(false);
            InputAction_Cancel?.Invoke();
        }

        [HeaderAttribute("==============螢幕效果==============")]
        [SerializeField]
        Transform ScreenEffectTrans;
        void InitScreenEffect() {
            ScreenEffectTrans.gameObject.SetActive(false);
        }
        public static void CallScreenEffect(string _name) {
            Instance.ScreenEffectTrans.gameObject.SetActive(true);
            Instance.ScreenEffectTrans.Find(_name).gameObject.SetActive(true);
        }
        public void OnScreenEffectEnd() {
            foreach (Transform trans in ScreenEffectTrans) {
                trans.gameObject.SetActive(false);
            }
        }





        [HeaderAttribute("==============事件短暫彈窗==============")]
        [SerializeField]
        PopupEventItem PopupItemPrefab = null;
        [SerializeField]
        Transform PopupEventParent = null;
        PopupEventSpawner MyPopupEventSpawner;
        public static bool CanShowEvent { get; set; } = true;

        void InitPopupEvent() {
            PopupEventParent.gameObject.SetActive(true);
            MyPopupEventSpawner = gameObject.AddComponent<PopupEventSpawner>();
            MyPopupEventSpawner.ParentTrans = PopupEventParent;
            MyPopupEventSpawner.ItemPrefab = PopupItemPrefab;
        }

        public static void ShowPopupEvent(string _text) {
            if (!Instance)
                return;
            if (!CanShowEvent)
                return;
            Vibrator.Vibrate(GameSettingData.GetInt(GameSetting.PopupEventVibrationMilliSecs));//手機震動
            PopupEventItem item = Instance.GetAvailableItem();
            if (item == null)
                item = Instance.SpawnNewItem();
            item.Init(_text, null);
        }
        public static void ShowPopupEvent(string _title, Action<object[]> _action, params object[] _objects) {
            if (!Instance)
                return;
            if (!CanShowEvent)
                return;
            Vibrator.Vibrate(GameSettingData.GetInt(GameSetting.PopupEventVibrationMilliSecs));//手機震動
            PopupEventItem item = Instance.GetAvailableItem();
            if (item == null)
                item = Instance.SpawnNewItem();
            item.Init(_title, _action, _objects);
        }
        PopupEventItem SpawnNewItem() {
            return MyPopupEventSpawner.Spawn<PopupEventItem>();
        }
        PopupEventItem GetAvailableItem() {
            for (int i = 0; i < MyPopupEventSpawner.ItemList.Count; i++) {
                if (!MyPopupEventSpawner.ItemList[i].IsActive)
                    return MyPopupEventSpawner.ItemList[i];
            }
            return null;
        }

        [HeaderAttribute("==============設定視窗==============")]

        //進遊戲不先初始化，等到要用時才初始化UI
        [SerializeField] Transform SettingUIParent;
        [SerializeField] AssetReference SettingUIAsset;
        TheDoor.Main.SettingUI MySettingUI;

        void InitSettingUI(Action _ac) {
            PopupUI.ShowLoading(StringData.GetUIString("WaitForLoadingUI"));
            //初始化UI
            AddressablesLoader.GetPrefabByRef(Instance.SettingUIAsset, (prefab, handle) => {
                PopupUI.HideLoading();
                GameObject go = Instantiate(prefab);
                go.transform.SetParent(Instance.SettingUIParent);
                go.transform.localPosition = prefab.transform.localPosition;
                go.transform.localScale = prefab.transform.localScale;
                //RectTransform rect = go.GetComponent<RectTransform>();
                //rect.offsetMin = Vector2.zero;//Left、Bottom
                //rect.offsetMax = Vector2.zero;//Right、Top
                Instance.MySettingUI = go.GetComponent<TheDoor.Main.SettingUI>();
                Instance.MySettingUI.Init();
                Instance.MySettingUI.SetActive(false);
                _ac?.Invoke();
            }, () => { WriteLog.LogError("載入GameSettingUIAsset失敗"); });
        }
        public static void CallSettingUI() {
            if (!Instance)
                return;
            //判斷是否已經載入過此UI，若還沒載過就跳讀取中並開始載入
            if (Instance.MySettingUI != null) {
                Instance.MySettingUI.SetActive(true);
            } else {
                Instance.InitSettingUI(() => {
                    Instance.MySettingUI.SetActive(true);
                });
            }
        }



        [HeaderAttribute("==============RoleInfoUI==============")]
        //進遊戲不先初始化，等到要用時才初始化UI
        [SerializeField] AssetReference RoleInfoUIAsset;
        [SerializeField] Transform RoleInfoUIParent;
        RoleInfoUI MyRoleInfoUI = null;
        static bool IsLoadingRoleInfoAsset = false;//是否載入UI中

        static void InitRoleInfoUI(Action _ac = null) {
            if (IsLoadingRoleInfoAsset)
                return;
            IsLoadingRoleInfoAsset = true;
            //初始化UI
            AddressablesLoader.GetPrefabByRef(Instance.RoleInfoUIAsset, (prefab, handle) => {
                IsLoadingRoleInfoAsset = false;
                GameObject go = Instantiate(prefab);
                go.transform.SetParent(Instance.RoleInfoUIParent);
                RectTransform rect = go.GetComponent<RectTransform>();
                go.transform.localPosition = prefab.transform.localPosition;
                go.transform.localScale = prefab.transform.localScale;
                rect.offsetMin = Vector2.zero;//Left、Bottom
                rect.offsetMax = Vector2.zero;//Right、Top
                go.transform.SetAsLastSibling();
                Instance.MyRoleInfoUI = go.GetComponent<RoleInfoUI>();
                Instance.MyRoleInfoUI.gameObject.SetActive(true);
                Instance.MyRoleInfoUI.Init();
                Instance.MyRoleInfoUI.SetActive(false);
                _ac?.Invoke();
            }, () => { WriteLog.LogError("載入RoleInfoUIAsset失敗"); });
        }
        public static void ShowRoleInfoUI(OwnedRoleData _ownedData, bool _showGoAdventureBtn = false) {
            if (Instance == null) return;
            if (Instance.MyRoleInfoUI != null) {
                Instance.MyRoleInfoUI.ShowUI(_ownedData, _showGoAdventureBtn);
            } else {
                PopupUI.ShowLoading(StringData.GetUIString("Loading"));
                InitRoleInfoUI(() => {
                    PopupUI.HideLoading();
                    Instance.MyRoleInfoUI.ShowUI(_ownedData, _showGoAdventureBtn);
                });
            }
        }

        [HeaderAttribute("==============獲得物品視窗==============")]

        //進遊戲不先初始化，等到要用時才初始化UI
        [SerializeField] AssetReference GainItemListUIAsset;
        [SerializeField] Transform GainItemListUIParent;
        GainItemListUI MyGainItemListUI;

        void InitGainItemListUI(Action _ac) {
            PopupUI.ShowLoading(StringData.GetUIString("WaitForLoadingUI"));
            //初始化UI
            AddressablesLoader.GetPrefabByRef(Instance.GainItemListUIAsset, (prefab, handle) => {
                PopupUI.HideLoading();
                GameObject go = Instantiate(prefab);
                go.transform.SetParent(Instance.GainItemListUIParent);
                go.transform.localPosition = prefab.transform.localPosition;
                go.transform.localScale = prefab.transform.localScale;
                RectTransform rect = go.GetComponent<RectTransform>();
                rect.offsetMin = Vector2.zero;//Left、Bottom
                rect.offsetMax = Vector2.zero;//Right、Top
                go.transform.SetAsLastSibling();
                Instance.MyGainItemListUI = go.GetComponent<GainItemListUI>();
                MyGainItemListUI.SetActive(false);
                Instance.MyGainItemListUI.Init();
                MyGainItemListUI.LoadItemAsset(_ac);
            }, () => { WriteLog.LogError("載入GainItemListUIAsset失敗"); });


        }
        public static void ShowGainItemListUI(string _title, List<ItemData> _itemDatas, List<ItemData> _replacedItems, Action _cb = null) {
            if (!Instance)
                return;

            //判斷是否已經載入過此UI，若還沒載過就跳讀取中並開始載入
            if (Instance.MyGainItemListUI != null) {
                Instance.MyGainItemListUI.ShowUI(_title, _itemDatas, _replacedItems, _cb);
            } else {
                Instance.InitGainItemListUI(() => {
                    Instance.MyGainItemListUI.ShowUI(_title, _itemDatas, _replacedItems, _cb);
                });
            }
        }



    }
}
