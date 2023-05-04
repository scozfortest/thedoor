using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using TheDoor.Main;
using SimpleJSON;

namespace Scoz.Func {
    public partial class Debugger : MonoBehaviour {

        public static Animator MyAni;
        // Update is called once per frame
        void KeyDetector() {


            if (Input.GetKeyDown(KeyCode.Q)) {

                LocoServerManager.RemoveCurUseRole();

            } else if (Input.GetKeyDown(KeyCode.W)) {


            } else if (Input.GetKeyDown(KeyCode.E)) {


            } else if (Input.GetKeyDown(KeyCode.R)) {


            } else if (Input.GetKeyDown(KeyCode.P)) {



            } else if (Input.GetKeyDown(KeyCode.O)) {

            } else if (Input.GetKeyDown(KeyCode.I)) {

            }
        }

    }
}
