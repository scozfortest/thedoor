using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Scoz.Func {
    public class ScreenTouchEffect : MonoBehaviour {
        private void Update() {
            Vector2 pos = Vector2.zero;
            if (Application.platform == RuntimePlatform.WindowsEditor) {
                if (Input.GetMouseButtonDown(0)) {
                    pos = Input.mousePosition;
                    SpawnEffect(pos);
                }
            } else {
                if (Input.touchCount == 1)//單點觸碰
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began) {
                        pos = Input.GetTouch(0).position;
                        SpawnEffect(pos);
                    }
                }
            }
        }

        void SpawnEffect(Vector2 _pos) {
        }
    }
}