using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.EventSystems;

namespace TheDoor.Main {
    public class SupplyPrefab : MonoBehaviour, IItem, IPointerDownHandler {
        [SerializeField] Image Icon;
        [SerializeField] Image CardBG;
        [SerializeField] TextMeshProUGUI Description;
        [SerializeField] TextMeshProUGUI Usage;
        [SerializeField] TextMeshProUGUI Time;
        [SerializeField] Material OutlineMaterial;
        [SerializeField] float VerticalDistToDragCard = 200;

        OwnedSupplyData OwnedData;
        enum DragState {
            Start,
            Dragging,
            End,
        }
        DragState CurDragState = DragState.End;
        Vector2 StartPos;

        public bool IsActive { get; set; }

        public void SetData(OwnedSupplyData _data) {
            OwnedData = _data;
            CardBG.material = null;
            Refresh();
        }
        public void Refresh() {
            var data = SupplyData.GetData(OwnedData.ID);
            Description.text = data.Description;
            Usage.text = OwnedData.Usage.ToString();
            Time.text = data.Time.ToString();
            AssetGet.GetSpriteFromAtlas(SupplyData.DataName + "Icon", data.Ref, sprite => {
                Icon.sprite = sprite;
            });
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (BattleManager.CurBattleState != BattleState.PlayerTurn) return;//玩家操作階段才可以使用卡牌
            StartPos = eventData.position;
            CurDragState = DragState.Start;
        }
        private void Update() {
            if (CurDragState == DragState.Start) {
                if (Mathf.Abs(((Vector2)Input.mousePosition - StartPos).y) > VerticalDistToDragCard) {
                    BattleUI.Instance.StartDrag(transform, UseSupplyToTarget);
                    CurDragState = DragState.Dragging;
                    CardBG.material = OutlineMaterial;
                }
            }
            if (CurDragState != DragState.End && Input.GetMouseButtonUp(0)) {
                CurDragState = DragState.End;
                BattleUI.Instance.EndDrag();
                CardBG.material = null;
            }
        }

        void UseSupplyToTarget(string _name) {

            var data = SupplyData.GetData(OwnedData.ID);
            WriteLog.LogFormat("對{0}部位 使用道具: {1}", _name, data.Name);
            AttackPart attackPart = MyEnum.ParseEnum<AttackPart>(_name);
            PlayerAction pAction = data.GetAction(BattleManager.PRole, BattleManager.ERole, attackPart);
            BattleManager.PlayerDoAction(pAction);
            OwnedData.AddUsage(-1);
            BattleUI.Instance?.RefreshSupplyUI();
            //GamePlayer.Instance.SaveToLoco_SupplyData();

        }

    }


}
