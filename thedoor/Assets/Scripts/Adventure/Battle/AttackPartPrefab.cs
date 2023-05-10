using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Scoz.Func;

namespace TheDoor.Main {
    public class AttackPartPrefab : MonoBehaviour {
        [SerializeField] AttackPart MyAttackPart;
        [SerializeField] Image Icon;
        [SerializeField] TextMeshProUGUI DmgText;
        [SerializeField] TextMeshProUGUI ProbText;

        public void RefreshUI() {
            AddressablesLoader.GetSpriteAtlas("AdventureUI", atlas => {
                Icon.sprite = atlas.GetSprite(MyAttackPart.ToString());
            });
            var partTurple = BattleManager.ERole.MyData.GetAttackPartTuple(MyAttackPart);
            DmgText.text = string.Format(StringData.GetUIString("AttackPart_Dmg"), TextManager.FloatToPercentStr(partTurple.Item1, 0));
            ProbText.text = string.Format(StringData.GetUIString("AttackPart_Prob"), TextManager.FloatToPercentStr(partTurple.Item2, 0));
        }

    }
}