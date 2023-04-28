using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public class AdventureManager {
        public static OwnedAdventureData CurAdventureData { get { return GamePlayer.Instance.Data.CurAdventure; } }
        public static DoorData CurDoorData { get { return GamePlayer.Instance.Data.CurAdventure.CurDoor; } }
        public static DoorType CurDoorType { get { return CurDoorData.DoorType; } }
        public static PlayerRole PRole { get; private set; }
        public static EnemyRole ERole { get; private set; }

        public static void CreatePlayerRole() {
            //建立玩家冒險用腳色
            var ownedPlayerData = GamePlayer.Instance.Data.CurRole;
            var roleData = RoleData.GetData(ownedPlayerData.ID);
            PRole = new PlayerRole.Builder()
                .SetData(roleData)
                .SetMaxHP(roleData.HP)
                .SetCurHP(ownedPlayerData.CurHP)
                .SetMaxSanP(roleData.SanP)
                .SetCurSanP(ownedPlayerData.CurSanP)
                .Build();
        }
        public static void SetEnemyRole() {
            MonsterData mData = MonsterData.GetData(Convert.ToInt32(CurDoorData.Values["MonsterID"]));
            ERole = new EnemyRole.Builder()
                .SetData(mData)
                .SetMaxHP(mData.HP)
                .SetCurHP(mData.HP)
                .Build();
        }

        public static void GoNextDoor() {
            CurAdventureData.NextDoor();
            PlayTransition();
        }
        static void PlayTransition() {
            AssetGet.GetImg("Door", "door1", sprite => {
                var transitionUI = TransitionDoorUI.GetInstance<TransitionDoorUI>();
                transitionUI?.CallTransition(sprite, "測試文字", 2, () => {
                    OpeTheDoor();
                });
            });
        }

        static void OpeTheDoor() {
            var adventureUI = AdventureUI.GetInstance<AdventureUI>();
            Debug.Log("CurDoorType=" + CurDoorType);
            switch (CurDoorType) {
                case DoorType.Encounter:
                    var scriptUI = ScriptUI.GetInstance<ScriptUI>();
                    scriptUI.LoadScript(CurDoorData.Values["ScriptTitleID"].ToString());
                    adventureUI.SwitchUI(AdventureUIs.Script);
                    break;
                case DoorType.Rest:
                    adventureUI.SwitchUI(AdventureUIs.Rest);
                    break;
                case DoorType.Monster:
                    SetEnemyRole();
                    adventureUI.SwitchUI(AdventureUIs.Battle);
                    break;
                case DoorType.Boss:
                    SetEnemyRole();
                    adventureUI.SwitchUI(AdventureUIs.Battle);
                    break;
                default:
                    break;
            }
        }


    }
}