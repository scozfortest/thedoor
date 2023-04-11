using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using Scoz.Func;

namespace TheDoor.Main {
    public class GameTimer : MonoBehaviour {
        MyTimer MinuteTimer;
        static DateTime LastOverMidNightTime;
        public static GameTimer Instance;

        public void Init() {
            Instance = this;
            MinuteTimer = new MyTimer(60, DoMinteThings, true, true);
        }
        private void Update() {
            if (MinuteTimer != null)
                MinuteTimer.RunTimer();
        }
        /// <summary>
        /// 剛載入完Firebase後，會先執行計時器第一次要執行的東西
        /// </summary>
        public void StartDoFirstAction() {
            DoMinteThings();
        }
        void DoMinteThings() {

            //Firebase相關執行要在登入狀態時才會跑
            if (FirebaseManager.MyUser != null)
                FirebaseRelated();

        }

        void FirebaseRelated() {
            //送server目前在線的時間戳
            bool sendOnlineCheck = FirestoreGameSetting.GetBoolData(GameDataDocEnum.Timer, "SendOnlineCheck");
            if (sendOnlineCheck) {

                int onlineTimeStampCD = FirestoreGameSetting.GetIntData(GameDataDocEnum.Timer, "OnlineTimeStampCD");
                int onlineTimeStampCDSecs = onlineTimeStampCD * 60;//onlineTimeStampCD是分鐘，要乘60轉秒數
                if (CDChecker.DoneCD("OnlineTimeStamp", onlineTimeStampCDSecs)) {//CD結束就送Timestamp
                    FirebaseManager.PlayerSign_UpdateOnlineTimestamp();//送更新Timestamp
                }
            }
            //取得每日相關資料(距離上一次更新不是同一天就會執行)
            if (LastOverMidNightTime == default(DateTime))
                LastOverMidNightTime = GameManager.Instance.NowTime;
            if (GameManager.Instance.NowTime.Day != LastOverMidNightTime.Day) {//確認上一次更新是不是同一天
                LastOverMidNightTime = GameManager.Instance.NowTime;
                FirebaseManager.PlayerSign_SignIn();//送登入Log
            }
            if (GamePlayer.Instance.Data != null) GameStateManager.Instance.InGameCheckCanPlayGame();//檢測是否可繼續遊戲
            GameStateManager.Instance.InGameCheckScheduledInGameNotification();//檢測是否跳出遊戲內推播
        }

    }
}
