using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using System;
using System.Linq;
using TMPro;

namespace TheDoor.Main {

    public class BattleUI : BaseUI {


        public override void Init() {
            base.Init();
        }

        public void SetBattle() {


        }
        public void GoNextDoor() {
            AdventureManager.GoNextDoor();
        }

    }
}