using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace Scoz.Func {
    public class EffectPlayer : MonoBehaviour {
        public List<EffectData> EffectList;

        void Start() {
            if (EffectList != null) {
                for (int i = 0; i < EffectList.Count; i++) {
                    if (EffectList[i].StartEmit) {
                        GameObjSpawner.SpawnGameObjByAssetRef(EffectList[i].Particle, EffectList[i].Offset, EffectList[i].Roration, EffectList[i].TargetParent, null);
                    }
                }
            }
        }

        public void PlayEffect(string _effectKey) {
            if (EffectList == null) {
                Debug.LogWarning("EffectList is null");
                return;
            }
            for (int i = 0; i < EffectList.Count; i++) {
                if (EffectList[i].Name == _effectKey) {
                    GameObjSpawner.SpawnGameObjByAssetRef(EffectList[i].Particle, EffectList[i].Offset, EffectList[i].Roration, EffectList[i].TargetParent, null);
                    break;
                }
            }
        }

    }
}
