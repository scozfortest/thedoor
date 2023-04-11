using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scoz.Func
{
    public class Angle
    {
        public static float GetAngerFormTowPoint2D(Vector2 _form, Vector2 _to)
        {
            Vector2 vector = _form - _to;
            float angle = (float)((Mathf.Atan2(vector.x, vector.y) / Mathf.PI) * 180f);
            angle = -(angle - 90);
            angle = ToPositiveAngle(angle);
            return angle;
        }
        public static float GetSmallestAngerFormTowPoint2D(Vector2 _form, Vector2 _to)
        {
            Vector2 vector = _form - _to;
            float angle = (float)((Mathf.Atan2(vector.x, vector.y) / Mathf.PI) * 180f);
            angle = -(angle - 90);
            angle = ToPositiveAngle(angle);
            if (angle > 180)
                angle = 360 - angle;
            return angle;
        }
        public static float ToPositiveAngle(float _angle)
        {
            float angle = _angle;
            angle = angle % 360;
            if (angle < 0) angle += 360f;
            return angle;
        }
        public static float GetAngleFromVelocity(Vector2 _velocity)
        {
            float angle = Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg;
            return angle;
        }
        public static Vector3 GetEulerFromAngle(float _angle)
        {
            return Quaternion.AngleAxis(_angle, Vector3.forward) * Vector3.right;
        }
    }

}