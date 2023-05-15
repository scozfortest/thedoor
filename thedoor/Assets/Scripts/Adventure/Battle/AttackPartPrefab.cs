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

        bool ShowingAttackPartTip = false;

        public void RefreshUI() {
            AddressablesLoader.GetSpriteAtlas("AdventureUI", atlas => {
                Icon.sprite = atlas.GetSprite(MyAttackPart.ToString());
            });
            var partTurple = BattleManager.ERole.MyData.GetAttackPartTuple(MyAttackPart);
            DmgText.text = string.Format(StringData.GetUIString("AttackPart_Dmg"), TextManager.FloatToPercentStr(partTurple.Item1, 0));
            ProbText.text = string.Format(StringData.GetUIString("AttackPart_Prob"), TextManager.FloatToPercentStr(partTurple.Item2, 0));
        }


        public void ShowTip() {
            ShowingAttackPartTip = true;
            var partTurple = BattleManager.ERole.MyData.GetAttackPartTuple(MyAttackPart);
            string nameText = StringData.GetUIString("Attack" + MyAttackPart.ToString());
            string dmgText = string.Format(StringData.GetUIString("AttackPart_Dmg"), TextManager.FloatToPercentStr(partTurple.Item1, 0));
            string probText = string.Format(StringData.GetUIString("AttackPart_Prob"), TextManager.FloatToPercentStr(partTurple.Item2, 0));
            string contentText = probText + "\n" + dmgText;

            //顯示Tip
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, transform.GetComponent<RectTransform>().position);
            TipUI.Instance.Show(nameText, contentText, screenPosition, Vector2.down * 150);
        }

        public void HideTip() {
            if (ShowingAttackPartTip) {
                TipUI.Instance.SetActive(false);
                ShowingAttackPartTip = false;
            }
        }
    }
}