using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public class AdventureManager {
        public static OwnedAdventureData CurAdventureData { get { return GamePlayer.Instance.Data.CurAdventure; } }
        public static DoorData CurDoorData { get { return GamePlayer.Instance.Data.CurAdventure.CurDoor; } }
        public static DoorType CurDoorType { get { return CurDoorData.DoorType; } }

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

            switch (CurDoorType) {
                case DoorType.Encounter:
                    var scriptUI = ScriptUI.GetInstance<ScriptUI>();
                    scriptUI.LoadScript(CurDoorData.Values["ScriptTitleID"].ToString());
                    adventureUI.SwitchUI(AdventureUIs.Default);
                    break;
                case DoorType.Rest:
                    adventureUI.SwitchUI(AdventureUIs.Rest);
                    break;
                case DoorType.Monster:
                    adventureUI.SwitchUI(AdventureUIs.Battle);
                    break;
                case DoorType.Boss:
                    adventureUI.SwitchUI(AdventureUIs.Battle);
                    break;
                default:
                    break;
            }
        }


    }
}