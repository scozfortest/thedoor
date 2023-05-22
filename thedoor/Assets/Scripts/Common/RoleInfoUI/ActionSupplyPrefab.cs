using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.EventSystems;

namespace TheDoor.Main {
    public class ActionSupplyPrefab : SupplyPrefab, IPointerDownHandler {
        [SerializeField] float VerticalDistToDragCard = 50;

        OwnedSupplyData OwnedData;
        PlayerAction CurPAction;
        enum DragState {
            Start,
            Dragging,
            End,
        }
        ActionSupplyType MyActionSupplyType;
        DragState CurDragState = DragState.End;
        Vector2 StartPos;
        public enum ActionSupplyType {
            Usable,//可以使用的道具
            Info,//僅供查看的道具
        }

        public void SetData(OwnedSupplyData _data, ActionSupplyType _actionSupplyType) {
            OwnedData = _data;
            CardBG.material = null;
            MyActionSupplyType = _actionSupplyType;
            MySupplyData = SupplyData.GetData(_data.ID);
            Refresh();
        }
        public override void Refresh() {
            base.Refresh();
            Name.text = MySupplyData.Name;
            Description.text = MySupplyData.EffectDescription;
            Usage.gameObject.SetActive(!MySupplyData.BelongTiming(SupplyData.Timing.None));
            if (MySupplyData.Usage != -1) Usage.text = OwnedData.Usage.ToString();
            else Usage.text = StringData.GetUIString("InfiniteUsage");
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (MyActionSupplyType != ActionSupplyType.Usable) return;
            if (BattleManager.CurBattleState != BattleState.PlayerTurn) return;//玩家操作階段才可以使用卡牌
            StartPos = eventData.position;
            CurDragState = DragState.Start;
            CurPAction = GetPlayerAction("Head");
        }
        private void Update() {
            if (MyActionSupplyType != ActionSupplyType.Usable) return;
            if (CurDragState == DragState.Start) {
                if (Mathf.Abs(((Vector2)Input.mousePosition - StartPos).y) > VerticalDistToDragCard) {
                    AdventureUI.Instance.StartDrag(CurPAction, transform, UseSupplyToTarget);

                    CurDragState = DragState.Dragging;
                    CardBG.material = OutlineMaterial;
                }
            }
            if (CurDragState != DragState.End && Input.GetMouseButtonUp(0)) {
                CurDragState = DragState.End;
                AdventureUI.Instance.EndDrag();
                CardBG.material = null;
            }
        }

        PlayerAction GetPlayerAction(string _attackPartStr) {
            var data = SupplyData.GetData(OwnedData.ID);
            PlayerAction pAction = null;
            if (MySupplyData.MyTarget == Target.Myself && _attackPartStr == Target.Myself.ToString()) {
                pAction = data.GetAction(AdventureManager.PRole, AdventureManager.PRole, AttackPart.Body);
            } else if (MySupplyData.MyTarget == Target.Enemy) {
                AttackPart attackPart = MyEnum.ParseEnum<AttackPart>(_attackPartStr);
                pAction = data.GetAction(BattleManager.PRole, BattleManager.ERole, attackPart);
            }
            return pAction;
        }

        void UseSupplyToTarget(string _name) {

            var data = SupplyData.GetData(OwnedData.ID);
            WriteLog.LogFormat("對{0}部位 使用道具: {1}", _name, data.Name);
            CurPAction = GetPlayerAction(_name);
            if (AdventureManager.MyState == AdvState.Battle)
                BattleManager.PlayerDoAction(CurPAction);
            else
                CurPAction.DoAction();
            OwnedData.AddUsage(-1);
            //更新UI
            if (BattleUI.Instance.gameObject.activeInHierarchy)
                BattleUI.Instance?.RefreshSupplyUI();
            if (RestUI.Instance.gameObject.activeInHierarchy)
                RestUI.Instance?.RefreshUI();
            //GamePlayer.Instance.SaveToLoco_SupplyData();

        }

    }


}
