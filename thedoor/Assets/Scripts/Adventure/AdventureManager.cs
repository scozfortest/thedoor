using Scoz.Func;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public class AdventureManager {

        [SerializeField] BattleManager BattleManager;
        public static OwnedAdventureData CurAdventureData { get { return GamePlayer.Instance.Data.CurAdventure; } }
        public static DoorData CurDoorData { get { return GamePlayer.Instance.Data.CurAdventure.CurDoor; } }
        public static DoorType CurDoorType { get { return CurDoorData.DoorType; } }
        public static PlayerRole PRole { get; private set; }


        public static void CreatePlayerRole() {
            //建立玩家冒險用腳色
            var ownedPlayerData = GamePlayer.Instance.Data.CurRole;
            var roleData = RoleData.GetData(ownedPlayerData.ID);
            PRole = new PlayerRole.Builder()
                .SetData(GamePlayer.Instance.Data.CurRole, roleData)
                .SetCurHP(ownedPlayerData.CurHP)
                .SetCurSanP(ownedPlayerData.CurSanP)
                .Build();

            var supplyDatas = PRole.GetSupplyDatas();
            PRole.AddSupplyExtendAttribute(supplyDatas);//獲得道具增加生命/新智最大值的時候也會同時增加目前值
            PRole.GainSupplyEffects(supplyDatas);//獲得道具狀態
        }

        public static void GoNextDoor() {
            PRole.DoTimePass(1);//執行時間流逝效果
            CurAdventureData.NextDoor();
            PlayTransition();
        }
        static void PlayTransition() {
            DoorNodeUI.Instance?.ShowUI(GamePlayer.Instance.Data.CurRole.MyAdventure);
            var doorTypeData = DoorStyleData.GetRndDatas();
            AssetGet.GetImg("Door", doorTypeData.Ref, sprite => {
                var transitionUI = TransitionDoorUI.Instance;
                transitionUI?.CallTransition(sprite, doorTypeData.Description, 2, () => {
                    OpeTheDoor();
                });
            });
        }

        static void OpeTheDoor() {
            var adventureUI = AdventureUI.Instance;
            WriteLog.LogColor(CurDoorType + "事件", WriteLog.LogType.Adventure);
            switch (CurDoorType) {
                case DoorType.Encounter:
                    var scriptUI = ScriptUI.Instance;
                    scriptUI.LoadScript(CurDoorData.Values["ScriptTitleID"].ToString());
                    adventureUI.SwitchUI(AdventureUIs.Script);
                    break;
                case DoorType.Rest:
                    adventureUI.SwitchUI(AdventureUIs.Rest);
                    break;
                case DoorType.Monster:
                case DoorType.Boss:
                    CallBattle(Convert.ToInt32(CurDoorData.Values["MonsterID"]), null);
                    break;
                default:
                    break;
            }
        }
        public static void CallBattle(int _monsterID, List<ItemData> _rewardItems) {
            BattleManager.ResetBattle(PRole, _monsterID, _rewardItems);
            AdventureUI.Instance.SwitchUI(AdventureUIs.Battle);
        }
        public static void GameOver() {
            LocoServerManager.RemoveCurUseRole();
            PopupUI.CallSceneTransition(MyScene.LobbyScene);
        }


    }
}