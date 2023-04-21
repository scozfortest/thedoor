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
                Dictionary<string, object> advJsonData = new Dictionary<string, object>();
                advJsonData.Add("CurDoor", 3);
                FirebaseManager.UpdateAdventure(advJsonData, null, null, () => {
                    PopupUI.ShowClickCancel("更新完成", null);
                });
            } else if (Input.GetKeyDown(KeyCode.W)) {
                JSONObject jObj = new JSONObject();
                jObj.Add("ColName", "Monster");
                JSONObject JsObj2 = new JSONObject();
                JsObj2.Add("A", 1);
                JsObj2.Add("B", 2);
                jObj.Add("Items", JsObj2);
                Debug.Log(jObj.ToString());


                JSONNode jsNode = JSON.Parse(jObj.ToString());
                Debug.Log(jsNode["Items"].AsObject["A"]);

            } else if (Input.GetKeyDown(KeyCode.E)) {
                GamePlayer.Instance.Data.SaveToLoco();

            } else if (Input.GetKeyDown(KeyCode.R)) {

                GamePlayer.Instance.LoadDataFromLoco(LocoDataName.Player);

            } else if (Input.GetKeyDown(KeyCode.P)) {


            } else if (Input.GetKeyDown(KeyCode.O)) {

            } else if (Input.GetKeyDown(KeyCode.I)) {

            }
        }

    }
}
