using TheDoor.Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Scoz.Func {
    [Serializable]
    public struct BloomSetting {
        public float Threshold;
        public float Intensity;
        public Color TintColor;
    }

    public class PostProcessingManager : MonoBehaviour {

        [Serializable] public class BloomSettingDicClass : SerializableDictionary<MyScene, BloomSetting> { }
        [SerializeField] BloomSettingDicClass MyBloomSettingDic;//字典攝影機設定字典
        Volume MyVolume;

        public static PostProcessingManager Instance;

        public void Init() {
            Instance = this;
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
            DontDestroyOnLoad(gameObject);
            MyVolume = GetComponent<Volume>();
        }
        private void OnDestroy() {
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }
        void OnLevelFinishedLoading(Scene _scene, LoadSceneMode _mode) {
            if (MyBloomSettingDic == null || MyBloomSettingDic.Count == 0) return;
            MyVolume.enabled = GamePlayer.Instance.PostProcessing;
            if (!GamePlayer.Instance.PostProcessing)//沒開後製效果就不用處理後續
                return;
            SetBloom(_scene);

        }
        public void RefreshSetting() {
            if (!Instance) return;
            MyVolume.enabled = GamePlayer.Instance.PostProcessing;
            if (GamePlayer.Instance.PostProcessing)
                SetBloom(SceneManager.GetActiveScene());
        }

        /// <summary>
        /// 根據場景設定Bloom
        /// </summary>
        void SetBloom(Scene _scene) {
            if (MyVolume == null) return;
            Bloom bloom;
            MyVolume.profile.TryGet(out bloom);
            if (bloom == null) return;
            MyScene myScene;
            if (MyEnum.TryParseEnum(_scene.name, out myScene)) {
                if (!MyBloomSettingDic.ContainsKey(myScene)) return;
                bloom.intensity.value = MyBloomSettingDic[myScene].Intensity;
                bloom.threshold.value = MyBloomSettingDic[myScene].Threshold;
                bloom.tint.value = MyBloomSettingDic[myScene].TintColor;
            }
        }
    }
}