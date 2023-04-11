using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Scoz.Func {
    [RequireComponent(typeof(CanvasScaler))]
    public class CanvasScalerModifier : MonoBehaviour {
        [SerializeField] Vector2 ExpectedResolution = new Vector2(1080, 2340);
        float ExpectedRatio {
            get {
                return (float)ExpectedResolution.y / (float)ExpectedResolution.x;
            }
        }
        float RealRatio {
            get {
                return (float)Screen.height / (float)Screen.width;
            }
        }
        private void Awake() {
            CanvasScaler canvasCaler = GetComponent<CanvasScaler>();
            if (canvasCaler == null) return;
            if (ExpectedRatio >= RealRatio) {
                canvasCaler.matchWidthOrHeight = 1;
            } else {
                canvasCaler.matchWidthOrHeight = 0;
            }

        }
    }
}