using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Numerics;
using UnityEngine.Profiling;
using TheDoor.Main;
using UnityEngine.SceneManagement;

namespace Scoz.Func {

    public class FPSChecker : MonoBehaviour {
        [SerializeField] float InfoRefreshInterval = 0.5f;

        public static FPSChecker Instance;
        int FrameCount = 0;
        float PassTimeByFrames = 0.0f;
        float LastFrameRate = 0.0f;

        /// <summary>
        /// 初始化
        /// </summary>
        void Awake() {
            Instance = this;
        }
        public void Update() {
            FPSCalc();
        }
        void FPSCalc() {
            if (PassTimeByFrames < InfoRefreshInterval) {
                PassTimeByFrames += Time.deltaTime;
                FrameCount++;
            } else {
                //This code will break if you set your m_refreshTime to 0, which makes no sense.
                LastFrameRate = (float)FrameCount / PassTimeByFrames;
                FrameCount = 0;
                PassTimeByFrames = 0.0f;
            }
        }
        public static float GetFPS() {
            if (Instance == null) {
                DebugLogger.LogError("尚未初始化FPSChecker");
                return 0;
            }
            return Instance.LastFrameRate;
        }
    }
}