using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Numerics;
using UnityEngine.Profiling;
using TheDoor.Main;
using UnityEngine.SceneManagement;

namespace Scoz.Func {
    public partial class Debugger : MonoBehaviour {
        public static bool IsInit;
        public static Debugger Instance;
        [SerializeField] GameObject TestPanelGo = null;
        [SerializeField] Text EnvVersion;
        public Text TotalMemoryText = null;
        public Text Text_FPS;
        public bool IsLimitFPS;
        public int MaxFPS;
        public float InfoRefreshInterval = 0.5f;
        public Text VersionText;

        int FrameCount = 0;
        float PassTimeByFrames = 0.0f;
        float LastFrameRate = 0.0f;

        MyTimer InfoRefreshTimer = null;


        public Camera GetCamera() {
            return GetComponent<Camera>();
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public void Start() {
            Instance = this;
            GameManager.Instance.AddCamStack(GetComponent<Camera>());//將自己的camera加入到目前場景上的MainCameraStack中
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
            GetComponent<RectTransform>().sizeDelta = new UnityEngine.Vector2(Screen.width, Screen.height);
            VersionText.text = "Ver: " + Application.version;

        }
        void OnDestroy() {
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }

        void OnLevelFinishedLoading(Scene _scene, LoadSceneMode _mode) {
            GameManager.Instance.AddCamStack(GetComponent<Camera>());//將自己的camera加入到目前場景上的MainCameraStack中
        }

        public void UpdateFirebaseEnvVersionText() {
            if (FirebaseManager.MyFirebaseApp != null)
                EnvVersion.text = "Env: " + FirebaseManager.MyFirebaseApp.Options.ProjectId;
        }
        void InitDebugger() {
            IsInit = true;
            //限制FPS
            LimitFPS(IsLimitFPS, MaxFPS);
            DontDestroyOnLoad(gameObject);

        }
        public static Debugger CreateNewInstance() {
            if (Instance != null) {
                WriteLog.Log("Debugger之前已經被建立了");
            } else {
                GameObject prefab = Resources.Load<GameObject>("Prefabs/Common/Debugger");
                GameObject go = Instantiate(prefab);
                go.name = "Debugger";
                Instance = go.GetComponent<Debugger>();
                Instance.InitDebugger();
            }
            return Instance;
        }

        /// <summary>
        /// 除錯顯示控制
        /// </summary>
        public void ShowDebugPanel(bool _show) {
            TestPanelGo.SetActive(_show);
        }
        /// <summary>
        /// 是否限制FPS
        /// </summary>
        public void LimitFPS(bool _limit, int _fps) {
            if (_limit) {
                QualitySettings.vSyncCount = 0;  // VSync must be disabled
                Application.targetFrameRate = _fps;
            } else {
                QualitySettings.vSyncCount = 1;
            }
        }

        void FPSCalc() {
            if (Text_FPS == null || !Text_FPS.isActiveAndEnabled)
                return;
            if (PassTimeByFrames < InfoRefreshInterval) {
                PassTimeByFrames += Time.deltaTime;
                FrameCount++;
            } else {
                //This code will break if you set your m_refreshTime to 0, which makes no sense.
                LastFrameRate = (float)FrameCount / PassTimeByFrames;
                FrameCount = 0;
                PassTimeByFrames = 0.0f;
            }
            Text_FPS.text = string.Format("FPS:{0}", Mathf.Round(LastFrameRate).ToString());
        }
        void Update() {
            FPSCalc();
            KeyDetector();
            if (InfoRefreshTimer != null)
                InfoRefreshTimer.RunTimer();
        }

        public void ClearLocoData() {
            PlayerPrefs.DeleteAll();
        }

    }
}
