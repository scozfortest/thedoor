using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Scoz.Func {

    public class CameraManager : MonoBehaviour {
        public enum CamNames {
            Adventure,
            ClawMachine,
        }
        static CinemachineBrain MyCinemachineBrain;
        static Dictionary<CamNames, CinemachineVirtualCamera> VirtualCams = new Dictionary<CamNames, CinemachineVirtualCamera>();

        public static void SetCam(CinemachineBrain _brain) {
            if (_brain == null)
                return;
            MyCinemachineBrain = _brain;
            MyCinemachineBrain.m_DefaultBlend.m_Time = 2;
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