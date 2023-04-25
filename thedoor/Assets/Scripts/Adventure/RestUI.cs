using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Scoz.Func;
using System;
using TMPro;

namespace TheDoor.Main {

    public class RestUI : BaseUI {


        public void ShowUI() {
            RefreshUI();
            SetActive(true);
        }

        public override void RefreshUI() {
            base.RefreshUI();
        }
        public void GoNextDoor() {
            AdventureManager.GoNextDoor();
        }
    }
}