using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scoz.Func
{
    public static class Vector
    {
        public static Vector3 GetCenterOfVectors(params Vector3[] vectors)
        {
            Vector3 sum = Vector3.zero;
            if (vectors == null || vectors.Length == 0)
            {
                return sum;
            }

            foreach (Vector3 vec in vectors)
            {
                sum += vec;
            }
            return sum / vectors.Length;
        }
        public static Vector2 GetRandomVector2(float _min, float _max)
        {
            float randX = Random.Range(_min, _max);
            float randY = Random.Range(_min, _max);
            return new Vector2(randX, randY);
        }

        public static Vector2 GetRandomPosInBoxCol2D(BoxCollider2D _col)
        {
            Vector2 pos = _col.transform.position;
            float randX = Random.Range(-_col.size.x / 2, _col.size.x / 2);
            float randY = Random.Range(-_col.size.y / 2, _col.size.y / 2);
            pos += new Vector2(randX, randY);
            return pos;
        }
        public static Vector2 GetRandomPosInCircleCol2D(CircleCollider2D _col)
        {
            return Circle.GetRandomPosInCircle(_col.transform.position, _col.radius);
        }
        public static Vector2 GetRandomPosOnCircleCol2D(CircleCollider2D _col)
        {
            return Circle.GetRandomPosOmCircle(_col.transform.position, _col.radius);
        }
    }
}
