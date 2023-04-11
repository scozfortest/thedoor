using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
namespace Scoz.Func
{
    public class MD5
    {
        public static string GetMD5(string _str)
        {
            byte[] asciiBytes = ASCIIEncoding.ASCII.GetBytes(_str);
            byte[] hashedBytes = MD5CryptoServiceProvider.Create().ComputeHash(asciiBytes);
            string hashedString = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            return hashedString;
        }
    }
}
