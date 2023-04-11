using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scoz.Func
{
    public class Line
    {
        /// <summary>
        /// 依據X座標取得在兩點連成的直線上的座標點Y
        /// </summary>
        /// <returns></returns>
        public static float GetPointYOnLineOfTwoPointByPointX(Vector2 _pos1, Vector2 _pos2, float _x)
        {
            //(Ｘ2-Ｘ1)(Ｙ-Ｙ1)=(Ｙ2-Ｙ1)(Ｘ-Ｘ1)兩點式直線方程式
            float yPoint = ((_pos2.y - _pos1.y) * (_x - _pos1.x) / (_pos2.x - _pos1.x)) + _pos1.y;
            return yPoint;
        }
        /// <summary>
        /// 依據Y座標取得在兩點連成的直線上的座標點X
        /// </summary>
        public static float GetPointXOnLineOfTwoPointByPointY(Vector2 _pos1, Vector2 _pos2, float _y)
        {
            //(Ｘ2-Ｘ1)(Ｙ-Ｙ1)=(Ｙ2-Ｙ1)(Ｘ-Ｘ1)兩點式直線方程式
            float xPoint = (((_pos2.x - _pos1.x) * (_y - _pos1.y)) / (_pos2.y - _pos1.y)) + _pos1.x;
            return xPoint;
        }
        /// <summary>
        /// 取的連成的直線穿過一平面的交集點
        /// </summary>
        public static Vector3 GetPointOnPlaneByTwoPoints(Plane _plan, Vector3 _pos1, Vector3 _pos2)
        {
            Vector3 towPointDir = (_pos2 - _pos1).normalized;
            Vector3 pointOnPlane = _plan.ClosestPointOnPlane(Vector3.zero);
            Vector3 planDir = _plan.normal;
            float offset = Vector3.Dot(pointOnPlane, planDir);
            float t = (offset - Vector3.Dot(_pos1, planDir)) / Vector3.Dot(towPointDir, planDir);
            return _pos1 + (towPointDir * t);
        }
    }
}