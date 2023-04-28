
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Scoz.Func {
    public class MyMath : MonoBehaviour {
        public static float Round(float _num, int _decimalPlaces) {
            return Mathf.Round(_num * (float)(10 ^ _decimalPlaces)) / (float)(10 ^ _decimalPlaces);
        }
        /// <summary>
        /// 取得正1或負1
        /// </summary>
        public static int Get1OrMinus1() {
            int rand = Random.Range(0, 2);
            if (rand == 0)
                rand = -1;
            return rand;
        }
        /// <summary>
        /// 無條件捨去至小數X位數
        /// </summary>
        public static decimal RoundDown_Decimal(decimal _num, float _decimalPlaces) {
            var power = System.Convert.ToDecimal(Mathf.Pow(10, _decimalPlaces));
            return decimal.Floor(_num * power) / power;
        }
        public static float GetTopProportionInTotal(float _curRank, float _total) {
            float result = 0;
            result = _curRank / _total * 100;
            result = Mathf.Round(result);
            return result;
        }
        public static int GetNumber1DividedByNumber2(float _number1, float _number2) {
            return (int)(Mathf.Round(_number1 / _number2));
        }
        public static int GetNumber1TimesNumber2(float _number1, float _number2) {
            return (int)(Mathf.Round(_number1 * _number2));
        }
        public static int GetPositiveNegativeRandomNumber(int _range) {
            int random = Random.Range(-_range, _range + 1);
            return random;
        }
        public static float GetPositiveNegativeRandomNumber(float _range) {
            float random = Random.Range(-_range, _range);
            return random;
        }
        public static float Calculate_ReturnFloat(float _num1, float _num2, Operator _operator) {
            float result = 0;
            switch (_operator) {
                case Operator.Plus:
                    result = _num1 + _num2;
                    break;
                case Operator.Minus:
                    result = _num1 - _num2;
                    break;
                case Operator.Times:
                    result = _num1 * _num2;
                    break;
                case Operator.Divided:
                    if (_num2 == 0) {
                        result = 0;
                        WriteLog.LogWarning("不可除以0");
                    } else
                        result = _num1 / _num2;
                    break;
                case Operator.Equal:
                    result = Mathf.Round(_num2);
                    break;
            }
            return result;
        }
        public static int Calculate_ReturnINT(float _num1, float _num2, Operator _operator) {
            int result = 0;
            switch (_operator) {
                case Operator.Plus:
                    result = (int)Mathf.Round(_num1 + _num2);
                    break;
                case Operator.Minus:
                    result = (int)Mathf.Round(_num1 - _num2);
                    break;
                case Operator.Times:
                    result = (int)Mathf.Round(_num1 * _num2);
                    break;
                case Operator.Divided:
                    result = (int)Mathf.Round(_num1 / _num2);
                    break;
                case Operator.Equal:
                    result = (int)(Mathf.Round(_num2));
                    break;
            }
            return result;
        }
        public static bool IsInteger(float _number) {
            if (_number.ToString().IndexOf(".") == -1) {
                return true;
            } else {
                return false;
            }
        }

        public static float StringToNumber(string _str) {
            float result;
            float.TryParse(_str, out result);
            return result;
        }
        public static float StringToNumber(string _str, string _prefix, string _suffix) {
            float result;
            if (_prefix != "")
                _str = _str.Replace(_prefix, "");
            if (_suffix != "")
                _str = _str.Replace(_suffix, "");
            float.TryParse(_str, out result);
            return result;
        }
        /// <summary>
        /// ax2+bx+c=0的公式解 2a分支-b加減根號b平方減4ac
        /// </summary>
        public static double QuadraticForm(float _a, float _b, float _c, bool _pos) {
            var preRoot = Mathf.Pow(_b, 2) - (4 * _a * _c);
            if (preRoot < 0) {
                return double.NaN;
            } else {
                var sgn = _pos ? 1 : -1;
                return (sgn * Mathf.Sqrt(preRoot) - _b) / (2.0 * _a);
            }
        }
        /// <summary>
        /// 取得等加速度運動中的移動距離(s=vt+0.5at^2)
        /// </summary>
        public static float GetSFromUniformAcceleratedMotion(float _v, float _a, float _t) {
            return _v * _t + 0.5f * _a * Mathf.Pow(_t, 2);
        }
        public static Vector3 GetPosOrNegVector(Vector3 _vec) {
            return new Vector3((_vec.x >= 0) ? 1 : -1, (_vec.y >= 0) ? 1 : -1, (_vec.z >= 0) ? 1 : -1);
        }
        public static int GetPosOrNativeOne(float _value) {
            if (_value >= 0)
                _value = 1;
            else
                _value = -1;
            return (int)_value;
        }
        public static Dictionary<T, int> GetIntValueDicAddition<T>(Dictionary<T, int> _dic1, Dictionary<T, int> _dic2) {
            var result = _dic1.Concat(_dic2).GroupBy(d => d.Key)
             .ToDictionary(d => d.Key, d => (d.First().Value + d.Last().Value));
            return result;
        }
        /// <summary>
        /// 取得a+ar+ar^2+ar^3....+ar^(n-1)的總和，但r不能等於1
        /// </summary>
        public static float GetSumOfQ1(int _a, float _r, int _n) {
            if (_r == 1) {
                WriteLog.LogError("r不能等於1");
                return 0;
            }
            float s = _a * ((1 - Mathf.Pow(_r, _n)) / (1 - _r));
            return s;
        }
        public static long BytesToKB(long _bytes) {
            return _bytes / 1024;
        }
        public static float BytesToMB(long _bytes) {
            return (_bytes / 1024f) / 1024f;
        }
        public static float BytesToGB(long _bytes) {
            return _bytes / Mathf.Pow(1024, 3);
        }
        /// <summary>
        /// 取得二次貝茲曲線上的點
        /// </summary>
        public static Vector2 QuadraticBezierCurve(Vector2 _startPos, Vector2 _ctrlPos, Vector2 _endPos, float _t) {
            return Mathf.Pow(1 - _t, 2) * _startPos + 2 * (1 - _t) * _t * _ctrlPos + Mathf.Pow(_t, 2) * _endPos;
        }
    }
}
