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
        [SerializeField] TextMeshProUGUI Description;
        [SerializeField] TextMeshProUGUI Usage;
        [SerializeField] TextMeshProUGUI Time;
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
            StartPos = eventData.position;
            CurDragState = DragState.Start;
        }
        private void Update() {
            if (CurDragState == DragState.Start) {
                if (Mathf.Abs(((Vector2)Input.mousePosition - StartPos).y) > VerticalDistToDragCard) {
                    BattleUI.GetInstance<BattleUI>().StartDrag(transform);
                    CurDragState = DragState.Dragging;
                }
            }
            if (Input.GetMouseButtonUp(0)) {
                CurDragState = DragState.End;
                BattleUI.GetInstance<BattleUI>().EndDrag();
            }
        }

    }


}
