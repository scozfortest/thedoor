using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using System;
using System.Linq;
using TMPro;

namespace TheDoor.Main {

    public class EnemyUI : MonoBehaviour {
        [SerializeField] Image EnemyImg;
        [SerializeField] Image HP;
        [SerializeField] List<GameObject> HitGOs;//頭、身、四肢

        EnemyRole ERole;


        public void RefreshUI(EnemyRole _enemyRole) {
            ERole = _enemyRole;
            SetHP(ERole.HPRatio);

            AssetGet.GetImg("Monster", ERole.MyData.Ref, sprite => {
                EnemyImg.sprite = sprite;
            });
            if (ERole.MyData.HeadDmg != 0 && ERole.MyData.HeadProb != 0)
                HitGOs[0].SetActive(true);
            else
                HitGOs[0].SetActive(false);
            if (ERole.MyData.BodyDmg != 0 && ERole.MyData.BodyProb != 0)
                HitGOs[1].SetActive(true);
            else
                HitGOs[1].SetActive(false);
            if (ERole.MyData.LimbsDmg != 0 && ERole.MyData.LimbsProb != 0)
                HitGOs[2].SetActive(true);
            else
                HitGOs[2].SetActive(false);
        }

        public void SetHP(float _hp) {
            HP.fillAmount = _hp;
        }

    }
}
