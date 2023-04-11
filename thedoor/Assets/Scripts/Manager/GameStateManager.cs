using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Scoz.Func;
using System.Linq;
using UnityEngine.SceneManagement;

namespace TheDoor.Main {
    public class GameStateManager : MonoBehaviour {
        public static GameStateManager Instance;
        public int ScheduledInGameNotificationID = 0;
        public enum CanPlayGameState {
            NeedGetNewVersion,//玩家版本過低，須強制到商店更新
            GotNewVersion,//有新版本可以更新，建議到商店更新
            Maintain,//維護中，只有特定開發者能進遊戲
            Ban,//該玩家被封鎖了，不能進遊戲
            Available,//可以直接進行遊戲
        }
        public void Init() {
            Instance = this;
        }
        /// <summary>
        /// 檢查是否要顯示遊戲內推播通知
        /// </summary>
        public void InGameCheckScheduledInGameNotification() {
            if (SceneManager.GetActiveScene().name != MyScene.LobbyScene.ToString()) return;//在LobbyScene時才會執行
            bool enable = FirestoreGameSetting.GetBoolData(GameDataDocEnum.ScheduledInGameNotification, "Enable");
            if (enable == false) return;
            int id = FirestoreGameSetting.GetIntData(GameDataDocEnum.ScheduledInGameNotification, "ID");
            if (id <= ScheduledInGameNotificationID) return;
            DateTime startTime = FirestoreGameSetting.GetDateTime(GameDataDocEnum.ScheduledInGameNotification, "StartTime");
            if (startTime > GameManager.Instance.NowTime) return;
            DateTime endTime = FirestoreGameSetting.GetDateTime(GameDataDocEnum.ScheduledInGameNotification, "EndTime");
            if (endTime < GameManager.Instance.NowTime) return;
            ScheduledInGameNotificationID = id;
            string content = FirestoreGameSetting.GetStrData(GameDataDocEnum.ScheduledInGameNotification, "Content");
            PopupUI.ShowClickCancel(content, null);
        }
        /// <summary>
        /// ※登入並載完資源包後會依序執行
        /// 1. 判斷玩家版本，若版本低於最低遊戲版本則會跳強制更新(在MaintainExemptPlayerUIDs中的玩家不會跳更新)
        /// 2. 判斷玩家版本，若版本低於目前遊戲版本則會跳更新建議(在MaintainExemptPlayerUIDs中的玩家不會跳更新)
        /// 3. 判斷Maintain是否為true，若為true則不在MaintainExemptPlayerUIDs中的玩家都會跳維護中
        /// 4. 判斷該玩家是否被Ban，不是才能進遊戲
        /// </summary>
        public CanPlayGameState GetCanPlayGameState() {
            string minimumGameVersion = FirestoreGameSetting.GetStrData(GameDataDocEnum.Version, "MinimumGameVersion");
            string gameVersion = FirestoreGameSetting.GetStrData(GameDataDocEnum.Version, "GameVersion");
            bool maintain = FirestoreGameSetting.GetBoolData(GameDataDocEnum.Version, "Maintain");
            List<string> maintainExemptPlayerUIDs = FirestoreGameSetting.GetStrs(GameDataDocEnum.Version, "MaintainExemptPlayerUIDs");
            bool ban = GamePlayer.Instance.Data.Ban;
            //Debug.LogError("Application.version=" + Application.version);
            //Debug.LogError("minimumGameVersion=" + minimumGameVersion);
            //Debug.LogError("gameVersion=" + gameVersion);
            if (maintain) {
                if (!maintainExemptPlayerUIDs.Contains(GamePlayer.Instance.Data.UID))
                    return CanPlayGameState.Maintain;
            }
            if (!TextManager.AVersionGreaterEqualToBVersion(Application.version, minimumGameVersion)) {
                if (!maintainExemptPlayerUIDs.Contains(GamePlayer.Instance.Data.UID))//在MaintainExemptPlayerUIDs中的玩家不會跳更新
                    return CanPlayGameState.NeedGetNewVersion;
            }
            if (!TextManager.AVersionGreaterEqualToBVersion(Application.version, gameVersion)) {
                if (!maintainExemptPlayerUIDs.Contains(GamePlayer.Instance.Data.UID))//在MaintainExemptPlayerUIDs中的玩家不會跳更新
                    return CanPlayGameState.GotNewVersion;
            }
            if (ban) return CanPlayGameState.Ban;
            return CanPlayGameState.Available;
        }

        /// <summary>
        /// 剛開始進遊戲時檢測是否可以進遊戲用，不能進入遊戲就跳對應的通知視窗
        /// </summary>
        public void StartCheckCanPlayGame(Action _passAction) {
            var state = GetCanPlayGameState();
            switch (state) {
                case GameStateManager.CanPlayGameState.Available://可直接進行遊戲
                    _passAction?.Invoke();
                    break;
                case GameStateManager.CanPlayGameState.GotNewVersion://有新版本(不強制更新)
                    string url = FirestoreGameSetting.GetStrData(GameDataDocEnum.Version, "AppStoreURL");
                    if (Application.platform == RuntimePlatform.Android)
                        url = FirestoreGameSetting.GetStrData(GameDataDocEnum.Version, "GoogleStoreURL");
                    PopupUI_Local.ShowConfirmCancel(StringData.GetUIString(state.ToString()), () => {
                        //點確認就去商店更新並關閉遊戲
                        Application.OpenURL(url);
                        Application.Quit();
                    }, () => {
                        //點取消
                        _passAction?.Invoke();
                    });
                    break;
                case GameStateManager.CanPlayGameState.NeedGetNewVersion://有新版本(強制更新)
                    string url2 = FirestoreGameSetting.GetStrData(GameDataDocEnum.Version, "AppStoreURL");
                    if (Application.platform == RuntimePlatform.Android)
                        url2 = FirestoreGameSetting.GetStrData(GameDataDocEnum.Version, "GoogleStoreURL");
                    PopupUI_Local.ShowClickCancel(StringData.GetUIString(state.ToString()), () => {
                        //點擊後就去商店更新並關閉遊戲
                        Application.OpenURL(url2);
                        Application.Quit();
                    });
                    break;
                case GameStateManager.CanPlayGameState.Maintain://維護中
                    DateTime maintainTime = FirestoreGameSetting.GetDateTime(GameDataDocEnum.Version, "MaintainEndTime");
                    if (GameManager.Instance.NowTime < maintainTime) {
                        PopupUI_Local.ShowClickCancel(string.Format(StringData.GetUIString("MaintainWithTime"), maintainTime), () => {
                            //點擊後關閉遊戲
                            Application.Quit();
                        });
                    } else {
                        PopupUI_Local.ShowClickCancel(StringData.GetUIString("Maintain"), () => {
                            //點擊後關閉遊戲
                            Application.Quit();
                        });
                    }
                    break;
                case GameStateManager.CanPlayGameState.Ban://玩家被Ban
                    PopupUI_Local.ShowClickCancel(StringData.GetUIString(state.ToString()), () => {
                        //點擊後就跳至官方客服網頁並關閉遊戲
                        string customerServiceURL = FirestoreGameSetting.GetStrData(GameDataDocEnum.Version, "CustomerServiceURL");
                        Application.OpenURL(customerServiceURL);
                        Application.Quit();
                    });
                    break;
                default:
                    DebugLogger.LogError("尚未定義的CanPlayGameState類型: " + state);
                    break;
            }
        }

        /// <summary>
        /// 玩家玩到一半時檢測是否可以繼續進行遊戲用，並跳對應的通知視窗
        /// 1. 不在StartScene時，GameData-Setting>Version資料更新時被呼叫
        /// 2. 不在StartScene時，每1分鐘被呼叫
        /// 3. 玩家自己的PlayerData-Player更新且Ban為true時被呼叫
        /// </summary>
        public void InGameCheckCanPlayGame() {
            if (SceneManager.GetActiveScene().name == MyScene.StartScene.ToString()) return;//不在StartScene時才會執行
            var state = GetCanPlayGameState();
            switch (state) {
                case GameStateManager.CanPlayGameState.Available://可直接進行遊戲
                    break;
                case GameStateManager.CanPlayGameState.GotNewVersion://有新版本(不強制更新)
                    break;
                case GameStateManager.CanPlayGameState.NeedGetNewVersion://有新版本(強制更新)
                    if (SceneManager.GetActiveScene().name == MyScene.LobbyScene.ToString()) {//如果是在大廳才跳強制更新訊息
                        string url2 = FirestoreGameSetting.GetStrData(GameDataDocEnum.Version, "AppStoreURL");
                        if (Application.platform == RuntimePlatform.Android)
                            url2 = FirestoreGameSetting.GetStrData(GameDataDocEnum.Version, "GoogleStoreURL");
                        PopupUI_Local.ShowClickCancel(StringData.GetUIString(state.ToString()), () => {
                            //點擊後就去商店更新並關閉遊戲
                            Application.OpenURL(url2);
                            Application.Quit();
                        });
                    }
                    break;
                case GameStateManager.CanPlayGameState.Maintain://維護中
                    DateTime maintainTime = FirestoreGameSetting.GetDateTime(GameDataDocEnum.Version, "MaintainEndTime");
                    if (GameManager.Instance.NowTime < maintainTime) {
                        PopupUI_Local.ShowClickCancel(string.Format(StringData.GetUIString("MaintainWithTime"), maintainTime), () => {
                            //點擊後關閉遊戲
                            Application.Quit();
                        });
                    } else {
                        PopupUI_Local.ShowClickCancel(StringData.GetUIString("Maintain"), () => {
                            //點擊後關閉遊戲
                            Application.Quit();
                        });
                    }
                    break;
                case GameStateManager.CanPlayGameState.Ban://玩家被Ban
                    PopupUI_Local.ShowClickCancel(StringData.GetUIString(state.ToString()), () => {
                        //點擊後就跳至官方客服網頁並關閉遊戲
                        string customerServiceURL = FirestoreGameSetting.GetStrData(GameDataDocEnum.Version, "CustomerServiceURL");
                        Application.OpenURL(customerServiceURL);
                        Application.Quit();
                    });
                    break;
                default:
                    DebugLogger.LogError("尚未定義的CanPlayGameState類型: " + state);
                    break;
            }
        }
    }
}