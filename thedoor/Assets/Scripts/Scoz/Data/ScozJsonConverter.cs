using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.Reflection;
using System;

namespace Scoz.Func {
    public static class ScozJsonConverter {

        /// <summary>
        /// 將IScozJsonConvertible轉為json 轉換的對象是有標記為ScozJsonSerializableAttribute的屬性
        /// </summary>
        public static JSONObject ToScozJson<T>(this T _obj) where T : IScozJsonConvertible {
            if (_obj == null) return null;
            return GetJson(_obj);
        }
        /// <summary>
        /// 將class轉為json 轉換目標是有標記為ScozJsonSerializableAttribute的屬性
        /// </summary>
        static JSONObject GetJson(object _obj) {
            if (_obj == null) return null;
            JSONObject jsonObj = new JSONObject();
            PropertyInfo[] properties = _obj.GetType().GetProperties();

            foreach (PropertyInfo property in properties) {
                if (Attribute.IsDefined(property, typeof(ScozSerializableAttribute))) {
                    string propertyName = property.Name;
                    object propertyValue = property.GetValue(_obj);


                    if (propertyValue is IConvertible) {
                        if (propertyValue is string) {//字串
                            jsonObj.Add(propertyName, propertyValue.ToString());
                        } else if (propertyValue is bool) {//布林
                            jsonObj.Add(propertyName, (bool)propertyValue);
                        } else if (propertyValue.GetType().IsEnum) {//列舉
                            jsonObj.Add(propertyName, propertyValue.ToString());
                        } else if (IsNumeric(propertyValue)) {//數字
                            double numberValue = Convert.ToDouble(propertyValue);
                            jsonObj.Add(propertyName, new JSONNumber(numberValue));
                        } else if (propertyValue is DateTime) {
                            jsonObj.Add(propertyName, ((DateTime)propertyValue).ToString("o"));
                        } else {//其他
                            jsonObj.Add(propertyName, propertyValue.ToString());
                        }
                    } else if (propertyValue is IDictionary) {//Dictionary類
                        JSONObject jsonDictionary = new JSONObject();
                        foreach (DictionaryEntry entry in (IDictionary)propertyValue) {
                            string key = entry.Key.ToString();
                            object value = entry.Value;
                            var valueJsonNode = GetJson(value);
                            jsonDictionary.Add(key, valueJsonNode);
                        }
                        jsonObj.Add(propertyName, jsonDictionary);
                    } else if (propertyValue is IEnumerable) {//List類或Array類
                        JSONArray jsonArray = new JSONArray();
                        foreach (object item in (IList)propertyValue) {
                            jsonArray.Add(GetJson(item));
                        }
                        jsonObj.Add(propertyName, jsonArray);
                    } else { // 其他
                        jsonObj.Add(propertyName, GetJson(propertyValue));
                    }
                }
            }

            return jsonObj;
        }

        static bool IsNumeric(object value) {
            if (value == null) {
                return false;
            }
            TypeCode typeCode = Type.GetTypeCode(value.GetType());
            return typeCode >= TypeCode.SByte && typeCode <= TypeCode.Decimal;
        }
    }
}