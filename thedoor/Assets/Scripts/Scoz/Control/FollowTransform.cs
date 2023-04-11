using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scoz.Func
{
    public class FollowTransform : MonoBehaviour
    {//未完成寫一半發現用不到
        public Transform Target;
        public bool Pos;
        public bool Rot;
        public bool Scale;
        public Vector3 PosOffset;
        public Vector3 RotOffset;

        private void Start()
        {
            //RotOffset = Target.localRotation.eulerAngles - transform.localRotation.eulerAngles;
            PosOffset = Target.localPosition - transform.localPosition;
        }
        public void Update()
        {
            if (Pos)
            {
                transform.localPosition = Target.localPosition - PosOffset;
            }

            if (Rot)
            {
                Quaternion quat = Quaternion.Euler(Target.localRotation.eulerAngles + RotOffset);
                transform.localRotation = quat;
            }
            if (Scale)
                transform.localScale = Target.localScale;
        }
    }
}
