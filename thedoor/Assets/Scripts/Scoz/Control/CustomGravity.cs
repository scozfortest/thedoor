using UnityEngine;

namespace Scoz.Func {
    [RequireComponent(typeof(Rigidbody))]
    public class CustomGravity : MonoBehaviour {
        public float GravityScale {
            get;
            private set;
        } = 1.0f;

        Rigidbody MyRigid;

        public void SetGravityScale(params float[] _scales) {
            GravityScale = 1.0f;
            for (int i = 0; i < _scales.Length; i++)
                GravityScale += _scales[i];
            //DebugLogger.LogError("GravityScale=" + GravityScale);
        }

        void OnEnable() {
            MyRigid = GetComponent<Rigidbody>();
            MyRigid.useGravity = false;
        }

        void FixedUpdate() {
            Vector3 gravity = Physics.gravity * GravityScale;
            MyRigid.AddForce(gravity, ForceMode.Acceleration);
        }
    }
}