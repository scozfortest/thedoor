using UnityEngine;

namespace Scoz.Func
{
    [System.Serializable]
    public struct MinMax
    {
        public int X;
        public int Y;
        public MinMax(int _x, int _y)
        {
            X = _x;
            Y = _y;
        }
        public override string ToString()
        {
            return "Min:"+X.ToString() + " Max:" + Y.ToString();
        }
        public int GetRandInRange()
        {
            return Random.Range(X, Y);
        }
    }
}