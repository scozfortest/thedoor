using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scoz.Func
{
    public class AudioRecord : MonoBehaviour
    {
        static Dictionary<string, AudioClip> AudioDic;
        static AudioClip MyAudioClip;
        static int MaxRecordTime;
        static int SamplingRate = 12000;

        public static bool StartRecording(int _recordTime)
        {
            Microphone.End(null);
            MaxRecordTime = _recordTime;
            MyAudioClip = Microphone.Start(null, false, MaxRecordTime, SamplingRate);
            return (MyAudioClip) ? true : false;
        }

        public static AudioClip EndRecording(string _name)
        {
            if(_name=="")
            {
                Microphone.End(null);
                return null;
            }
            int lastPos = Microphone.GetPosition(null);
            float curRecordTime = 0;
            if (Microphone.IsRecording(null))
            {
                curRecordTime = lastPos / SamplingRate;
            }
            else
                return null;
            Microphone.End(null);
            if (curRecordTime < 1)
            {
                return null;
            }
            if (AudioDic == null)
                AudioDic = new Dictionary<string, AudioClip>();
            if (AudioDic.ContainsKey(_name))
            {
                AudioDic[_name] = MyAudioClip;
            }
            else
                AudioDic.Add(_name, MyAudioClip);
            return MyAudioClip;
        }
        public static AudioClip GetRecordAudio(string _name)
        {
            if (AudioDic.ContainsKey(_name))
            {
                return AudioDic[_name];
            }
            else
            {
                return null;
            }
        }
        public static void RemoveRecordAudioFromDic(string _name)
        {
            if (AudioDic.ContainsKey(_name))
            {
                AudioDic.Remove(_name);
            }
        }

    }

}
