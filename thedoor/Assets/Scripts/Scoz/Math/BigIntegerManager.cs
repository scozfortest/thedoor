using System;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using JetBrains.Annotations;
using System.Linq;

public class BigIntegerManager
{
    public static string GetStr(BigInteger _bi)
    {
        if (_bi == 0)
            return "0";
        BigNumberType type = BigNumberType.BelowK;
        string result = "";
        string suffix = "";
        string remainder = "";
        //正規化
        if (_bi >= 1000)
        {
            while (_bi >= 1000)
            {
                if (_bi < 1000000)
                {
                    remainder = ((decimal)_bi / 1000).ToString();
                    if (remainder.Contains("."))
                    {
                        remainder = remainder.Substring(remainder.LastIndexOf(".") + 1);
                    }
                    else
                        remainder = "";
                }
                _bi /= 1000;
                type++;
            }
        }
        if (type != BigNumberType.BelowK)
            suffix = type.ToString();
        if (remainder == "")
            result = string.Format("{0}{1}", _bi.ToString(), suffix);
        else
            result = string.Format("{0}.{1}{2}", _bi.ToString(), remainder, suffix);
        return result;
    }
    public static string GetNoPointStr(BigInteger _bi)
    {
        if (_bi == 0)
            return "0";
        BigNumberType type = BigNumberType.BelowK;
        string result = "";
        string suffix = "";
        //正規化
        if (_bi >= 1000)
        {
            while (_bi >= 1000)
            {
                _bi /= 1000;
                type++;
            }
        }
        if (type != BigNumberType.BelowK)
            suffix = type.ToString();
        result = string.Format("{0}{1}", _bi.ToString(), suffix);
        return result;
    }
    /// <summary>
    /// 從 類型:數字格式轉換為BigInteger
    /// </summary>
    public static BigInteger StrToBigInteger(string _str)
    {
        string[] strs = _str.Split(':');
        BigNumberType type = (BigNumberType)int.Parse(strs[0]);
        string[] numStrs = strs[1].Split('.');
        BigInteger bi = new BigInteger(0);
        if (numStrs.Length == 1)
            bi = new BigInteger((int.Parse(numStrs[0]) * Math.Pow(1000, (int)type)));
        else if (numStrs.Length == 2)
            bi = new BigInteger((int.Parse(numStrs[0]) * Math.Pow(1000, (int)type)) + int.Parse(numStrs[1]) * Math.Pow(1000, (int)type - 1));
        return bi;
    }
    /// <summary>
    /// 從 類型:數字格式轉換為數字類型格式 Ex. 2:50 -> 50M
    /// </summary>
    public static string StrToStr(string _str)
    {
        string[] strs = _str.Split(':');
        BigNumberType type = (BigNumberType)int.Parse(strs[0]);
        string s = strs[1] + type.ToString();
        return s;
    }
    /*
    public static BigInteger Str2ToBigInteger(string _str)
    {
        BigInteger bi = new BigInteger(0);
        string lastStr = _str.Last().ToString();
        bool isNumeric = int.TryParse(lastStr, out _);
        if(!isNumeric)
        {
            _str.Remove(_str.Length - 1);
        }
        return bi;
    }
    */
    /// <summary>
    /// 轉換成 類型:數字格式  ex. T:31.5
    /// </summary>
    public static string GetDataStr(BigInteger _bi)
    {
        if (_bi == 0)
            return string.Format("{0}:{1}", (int)BigNumberType.BelowK, 0);
        else
        {
            BigNumberType type = BigNumberType.BelowK;
            string remainder = "";
            //正規化
            if (_bi >= 1000)
            {
                while (_bi >= 1000)
                {
                    remainder = (_bi % 1000).ToString();
                    _bi /= 1000;
                    type++;
                }
            }
            if (remainder != "")
                return string.Format("{0}:{1}", (int)type, string.Format("{0}.{1}", _bi, remainder));
            else
                return string.Format("{0}:{1}", (int)type, _bi);
        }

    }
    public static BigInteger Multiply(BigInteger _bi, params float[] _values)
    {
        for (int i = 0; i < _values.Length; i++)
        {
            _bi = Multiply(_bi, _values[i]);
        }
        return _bi;
    }
    static BigInteger Multiply(BigInteger _bi, float _value)
    {
        var f = Fraction((decimal)_value);
        return _bi * f.numerator / f.denominator;
    }
    public static BigInteger Multiply(BigInteger _bi, decimal _value)
    {
        var f = Fraction(_value);
        return _bi * f.numerator / f.denominator;
    }
    static (BigInteger numerator, BigInteger denominator) Fraction(decimal d)
    {
        int[] bits = decimal.GetBits(d);
        BigInteger numerator = (1 - ((bits[3] >> 30) & 2)) *
                               unchecked(((BigInteger)(uint)bits[2] << 64) |
                                         ((BigInteger)(uint)bits[1] << 32) |
                                          (BigInteger)(uint)bits[0]);
        BigInteger denominator = BigInteger.Pow(10, (bits[3] >> 16) & 0xff);
        return (numerator, denominator);
    }
    public static double Divide(BigInteger _num1, BigInteger _num2)
    {
        double result = Math.Exp(BigInteger.Log(_num1) - BigInteger.Log(_num2));
        return result;
    }
    public static BigNumberType GetNumberType(BigInteger _num)
    {
        BigNumberType type = BigNumberType.BelowK;
        //正規化
        if (_num >= 1000)
        {
            while (_num >= 1000)
            {
                _num /= 1000;
                type++;
            }
        }
        return type;
    }

}
