using System;
using System.Collections.Generic;
using TheDoor.Main;
using UnityEngine;
using UnityEngine.UI;
namespace Scoz.Func {
    public class DragIndicator : MonoBehaviour {
        [SerializeField] GameObject NodePrefab; // 節點Prefab
        [SerializeField] GameObject Arrow;//箭頭
        [SerializeField] int NodeCount = 10; // 曲線上的圓形數量
        [SerializeField] float CurveHeight = 200; // 曲線的基本高度
        [SerializeField] float HeightVariation = 30; // 曲線高度的變化範圍
        [SerializeField] float Speed = 3; // 曲線變化的速度
        [SerializeField] List<Image> TargetImgs;//目標

        Transform StartTrans;//起始目標Transfrom
        bool IsDragging = false;
        List<GameObject> Nodes = new List<GameObject>();
        Canvas MyCanvas;
        Action<string> TargetNameCB;
        Action NotOnTargetCB;

        public void Init() {
            IsDragging = false;
            Arrow.SetActive(false);
            for (int i = 0; i < NodeCount; i++) {
                GameObject circle = Instantiate(NodePrefab, transform);
                Nodes.Add(circle);
            }
            MyCanvas = GetComponentInParent<Canvas>();
        }
        public void StartDrag(Transform _startTrans, Action<string> _onTargetCB, Action _notOnTargetCB) {
            StartTrans = _startTrans;
            TargetNameCB = _onTargetCB;
            NotOnTargetCB = _notOnTargetCB;
            IsDragging = true;
            Dragging();
            ShowNodes(true);
        }
        public void EndDrag() {
            IsDragging = false;
            CheckReleaseOnTarget();
            ShowNodes(false);
        }
        void CheckReleaseOnTarget() {
            foreach (var img in TargetImgs) {
                if (!img.gameObject.activeInHierarchy) continue;
                RectTransform targetRect = img.GetComponent<RectTransform>();
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRect, Input.mousePosition, MyCanvas.worldCamera, out localPoint);
                float radius = Mathf.Min(targetRect.rect.width, targetRect.rect.height) * 0.5f;
                if (Vector2.Distance(localPoint, targetRect.rect.center) <= radius) {
                    OnTarget(img.name);
                    return;
                }
            }
            NotOnTarget();
        }
        void OnTarget(string _name) {
            TargetNameCB?.Invoke(_name);
        }
        void NotOnTarget() {
            NotOnTargetCB?.Invoke();
        }
        void ShowNodes(bool _show) {
            foreach (var node in Nodes) node.SetActive(_show);
            Arrow.SetActive(_show);
        }

        void FixedUpdate() {
            Dragging();
        }

        void Dragging() {
            if (!IsDragging) return;
            Vector2 inputPosition = Vector2.zero;
            bool hasInput = false;

            if (Input.touchCount > 0) {
                Touch touch = Input.GetTouch(0);
                inputPosition = touch.position;
                hasInput = true;
            } else if (Input.GetMouseButton(0)) {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), Input.mousePosition, MyCanvas.worldCamera, out inputPosition);
                hasInput = true;
            }

            if (hasInput) {
                Vector2 startPosition = RectTransformUtility.WorldToScreenPoint(MyCanvas.worldCamera, StartTrans.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), startPosition, MyCanvas.worldCamera, out startPosition);
                Vector2 controlPoint = (startPosition + inputPosition) / 2;
                float dynamicCurveHeight = CurveHeight + Mathf.Sin(Time.time * Speed) * HeightVariation;
                controlPoint.y += dynamicCurveHeight;

                for (int i = 0; i < Nodes.Count; i++) {
                    if (i == Nodes.Count - 1) {
                        Nodes[i].SetActive(false);
                        break;
                    }
                    float t = (float)i / (Nodes.Count - 1);
                    Vector2 position = MyMath.QuadraticBezierCurve(startPosition, controlPoint, inputPosition, t);
                    Nodes[i].GetComponent<RectTransform>().anchoredPosition = position;
                }


                // 設置箭頭位置
                var arrowRectTrans = Arrow.GetComponent<RectTransform>();
                arrowRectTrans.anchoredPosition = inputPosition;
                // 旋轉箭頭以對準曲線的末端方向
                Vector2 direction = (Vector2)Nodes[Nodes.Count - 2].transform.position - (Vector2)Nodes[Nodes.Count - 3].transform.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                arrowRectTrans.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

    }
}