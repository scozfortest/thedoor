using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Scoz.Func {


    public class CameraManager : MonoBehaviour {

        [SerializeField] float CamShakeFrequencyGain = 1f;
        public static CameraManager Instance;

        public enum CamNames {
            Adventure,
        }
        static CinemachineBrain MyCinemachineBrain;
        static Dictionary<CamNames, CinemachineVirtualCamera> VirtualCams = new Dictionary<CamNames, CinemachineVirtualCamera>();
        public void Init() {
            Instance = this;
        }

        public static void SetCam(CinemachineBrain _brain) {
            if (_brain == null)
                return;
            MyCinemachineBrain = _brain;
            MyCinemachineBrain.m_DefaultBlend.m_Time = 2;
        }
        public static void ShakeCam(CamNames _camName, float _amplitudeGain, float _frequencyGain, float _duration) {
            CinemachineBasicMultiChannelPerlin perlin = GetVirtualCam(_camName)?.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (perlin != null) {
                perlin.m_AmplitudeGain = _amplitudeGain;
                perlin.m_FrequencyGain = _frequencyGain;
                CoroutineJob.Instance.StartNewAction(() => {
                    perlin.m_AmplitudeGain = 0;
                    MyCinemachineBrain.transform.localPosition = Vector3.zero;
                }, _duration);
            }
        }
        public static void AddVirtualCam(CamNames _camName, CinemachineVirtualCamera _cam) {
            if (!VirtualCams.ContainsKey(_camName))
                VirtualCams.Add(_camName, _cam);
            else
                VirtualCams[_camName] = _cam;
        }

        public static void RemoveVirtualCam(CamNames _name) {
            VirtualCams.Remove(_name);
        }
        public static CinemachineVirtualCamera GetVirtualCam(CamNames _key) {
            if (VirtualCams.ContainsKey(_key))
                return VirtualCams[_key];
            return null;
        }
        public static void ChangeCamTransitionTime(float _time) {
            if (MyCinemachineBrain == null)
                return;
            MyCinemachineBrain.m_DefaultBlend.m_Time = _time;
        }
    }
}