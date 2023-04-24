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
            return GetScozJson(_obj);
        }
        /// <summary>
        /// 將class轉為json 轉換目標是有標記為ScozJsonSerializableAttribute的屬性
        /// </summary>
        static JSONObject GetScozJson(object _classObj) {
            if (_classObj == null) return null;
            JSONObject jsonObj = new JSONObject();
            PropertyInfo[] properties = _classObj.GetType().GetProperties();
            foreach (PropertyInfo property in properties) {
                if (Attribute.IsDefined(property, typeof(ScozSerializableAttribute))) {
                    string propertyName = property.Name;
                    object propertyValue = property.GetValue(_classObj);

                    if (propertyValue is IConvertible) {
                        SetIConvertibleToJSONObj(jsonObj, propertyName, (IConvertible)propertyValue);
                    } else if (propertyValue is IDictionary) {//Dictionary類
                        JSONObject jsonDictionary = new JSONObject();
                        SetIDicToJSONObj(jsonDictionary, (IDictionary)propertyValue);
                        jsonObj.Add(propertyName, jsonDictionary);
                    } else if (propertyValue is IEnumerable) {//List類或Array類
                        JSONArray jsonArray = new JSONArray();
                        SetIEnumerableToJSONArray(jsonArray, (IEnumerable)propertyValue);
                        jsonObj.Add(propertyName, jsonArray);
                    } else { // 其他
                        jsonObj.Add(propertyName, GetScozJson(propertyValue));
                    }
                }
            }

            return jsonObj;
        }

        static void SetIConvertibleToJSONObj(JSONObject _jsObj, string _key, IConvertible _iConvertible) {
            if (_iConvertible is string) {//字串
                _jsObj.Add(_key, _iConvertible.ToString());
            } else if (_iConvertible is bool) {//布林
                _jsObj.Add(_key, (bool)_iConvertible);
            } else if (_iConvertible.GetType().IsEnum) {//列舉
                _jsObj.Add(_key, _iConvertible.ToString());
            } else if (IsNumeric(_iConvertible)) {//數字
                double numberValue = Convert.ToDouble(_iConvertible);
                _jsObj.Add(_key, new JSONNumber(numberValue));
            } else if (_iConvertible is DateTime) {
                _jsObj.Add(_key, ((DateTime)_iConvertible).ToString("o"));
            } else {//其他
                _jsObj.Add(_key, _iConvertible.ToString());
            }
        }
        static void SetIConvertibleToJSONArray(JSONArray _jsArray, IConvertible _iConvertible) {
            if (_iConvertible is string) {//字串
                _jsArray.Add(_iConvertible.ToString());
            } else if (_iConvertible is bool) {//布林
                _jsArray.Add((bool)_iConvertible);
            } else if (_iConvertible.GetType().IsEnum) {//列舉
                _jsArray.Add(_iConvertible.ToString());
            } else if (IsNumeric(_iConvertible)) {//數字
                double numberValue = Convert.ToDouble(_iConvertible);
                _jsArray.Add(new JSONNumber(numberValue));
            } else if (_iConvertible is DateTime) {
                _jsArray.Add(((DateTime)_iConvertible).ToString("o"));
            } else {//其他
                _jsArray.Add(_iConvertible.ToString());
            }
        }
        static void SetIDicToJSONObj(JSONObject _jsObj, IDictionary _iDic) {
            foreach (DictionaryEntry entry in _iDic) {
                string key = entry.Key.ToString();
                object value = entry.Value;
                if (value is IConvertible) {
                    SetIConvertibleToJSONObj(_jsObj, key, (IConvertible)value);
                } else if (value is IDictionary) {
                    JSONObject subJsObj = new JSONObject();
                    SetIDicToJSONObj(subJsObj, (IDictionary)value);
                    _jsObj.Add(key, subJsObj);
                } else if (value is IEnumerable) {
                    JSONArray subJsArray = new JSONArray();
                    SetIEnumerableToJSONArray(subJsArray, (IEnumerable)value);
                    _jsObj.Add(key, subJsArray);
                } else {
                    var valueJsonNode = GetScozJson(value);
                    _jsObj.Add(key, valueJsonNode);
                }
            }
        }
        static void SetIEnumerableToJSONArray(JSONArray _jsArray, IEnumerable _iEnumerable) {
            foreach (object value in (IList)_iEnumerable) {

                if (value is IConvertible) {
                    SetIConvertibleToJSONArray(_jsArray, (IConvertible)value);
                } else if (value is IDictionary) {
                    JSONObject subJsObj = new JSONObject();
                    SetIDicToJSONObj(subJsObj, (IDictionary)value);
                    _jsArray.Add(subJsObj);
                } else if (value is IEnumerable) {
                    JSONArray subJsArray = new JSONArray();
                    SetIEnumerableToJSONArray(subJsArray, (IEnumerable)value);
                    _jsArray.Add(subJsArray);
                } else {
                    var valueJsonNode = GetScozJson(value);
                    _jsArray.Add(valueJsonNode);
                }
            }
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