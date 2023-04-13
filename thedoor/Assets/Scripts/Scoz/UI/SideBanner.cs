using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using TheDoor.Main;
using UnityEngine.AddressableAssets;

namespace Scoz.Func {
    public class SideBanner : MonoBehaviour {
        static SideBanner Instance;

        public static SideBanner CreateNewInstance() {
            if (Instance != null) {
                WriteLog.LogError("GameDictionary之前已經被建立了");
            } else {
                GameObject prefab = Resources.Load<GameObject>("Prefabs/Common/SideBanner");
                GameObject go = Instantiate(prefab);
                go.name = "SideBanner";
                Instance = go.GetComponent<SideBanner>();
                Instance.Init();
            }
            return Instance;
        }
        public void Init() {
            GameManager.Instance.AddCamStack(GetComponent<Camera>());//將自己的camera加入到目前場景上的MainCameraStack中
            DontDestroyOnLoad(gameObject);
        }
        void Start() {
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }

        void OnDestroy() {
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }
        void OnLevelFinishedLoading(Scene _scene, LoadSceneMode _mode) {
            GameManager.Instance.AddCamStack(GetComponent<Camera>());//將自己的camera加入到目前場景上的MainCameraStack中
        }
    }
}