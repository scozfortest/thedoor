using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using System;
using System.Linq;

namespace TheDoor.Main {

    public class LobbyUI : BaseUI {

        public LobbyUIs CurUI { get; private set; } = LobbyUIs.Lobby;
        public BaseUI LastPopupUI { get; private set; }//紀錄上次的彈出介面(切介面時要關閉上次的彈出介面)

        static bool FirstEnterLobby = true;//第一次進入大廳後會設定回false 用來判斷是否第一次進入大廳而做判斷

        public override void Init() {
            base.Init();
        }

        public void GoToHell() {
            PopupUI.InitSceneTransitionProgress(1, "AdventureUILoaded");
            PopupUI.CallTransition(MyScene.AdventureScene);
        }

    }
}