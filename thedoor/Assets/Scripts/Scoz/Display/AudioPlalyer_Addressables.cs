/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.AddressableAssets;
namespace Scoz.Func
{

    public partial class AudioPlayer : MonoBehaviour
    {
        public static void PlayAudioByString_static(AudioType _type, string _name)
        {
            if (!IsInit)
                InstantiateAudioManager();
            switch (_type)
            {
                case AudioType.Music:
                    if (GetApplicableMusicSource() == null)
                        GetNewMusicSource();
                    break;
                case AudioType.Sound:
                    if (GetApplicableSoundSource() == null)
                        GetNewSoundSource();
                    break;
                case AudioType.Voice:
                    if (GetApplicableVoiceSource() == null)
                        GetNewVoiceSource();
                    break;
            }


            AddressablesLoader.GetAudio(_type, _name, a =>
            {
                if (a != null)
                {
                    PlayAudio(_type, CurPlayAudio, a);
                }
                else
                {
                    DebugLogger.LogWarning("不存在的音檔:" + _name);
                    return;
                }
            });
        }
    }
}
*/