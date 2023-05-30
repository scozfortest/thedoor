using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using TheDoor.Main;
using SimpleJSON;

namespace Scoz.Func {
    public partial class Debugger : MonoBehaviour {

        [SerializeField] GameObject ToolGO;

        public static Animator MyAni;
        // Update is called once per frame
        void KeyDetector() {


            if (Input.GetKeyDown(KeyCode.Q)) {
                MyVideoPlayer.Instance.PlayVideo("https://drive.google.com/uc?export=download&id=1wqHVcMzhevT891tE_IuFjg_oW-0gEHpY", false);


            } else if (Input.GetKeyDown(KeyCode.W)) {


            } else if (Input.GetKeyDown(KeyCode.E)) {


            } else if (Input.GetKeyDown(KeyCode.R)) {


            } else if (Input.GetKeyDown(KeyCode.P)) {
                ToolGO?.SetActive(!ToolGO.activeSelf);


            } else if (Input.GetKeyDown(KeyCode.O)) {

            } else if (Input.GetKeyDown(KeyCode.I)) {

            }
        }

        public void OnModifyHP(int _value) {
            AdventureManager.PRole.AddHP(_value);
        }
        public void OnModifySanP(int _value) {
            AdventureManager.PRole.AddSanP(_value);
        }
        public void ClearLocoData() {
            PlayerPrefs.DeleteAll();
        }

    }
}
