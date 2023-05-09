using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

namespace Scoz.Func {
    public class Prob {
        public static bool GetResult(float _probability) {
            int randomNum = Random.Range(0, 100);
            _probability = Mathf.Clamp(_probability, 0, 1);
            if (randomNum < Mathf.RoundToInt(_probability * 100))
                return true;
            else
                return false;
        }
        public static int GetIndexFromWeigth(List<int> _weigthList) {
            int allWeigth = 0;
            for (int i = 0; i < _weigthList.Count; i++) {
                allWeigth += _weigthList[i];
            }
            int randNum = Random.Range(0, allWeigth);
            for (int i = 0; i < _weigthList.Count; i++) {
                allWeigth -= _weigthList[i];
                if (allWeigth <= randNum) {
                    return i;
                }
            }
            WriteLog.LogWarning("權重取得器錯誤");
            return _weigthList.Count - 1;
        }
        /// <summary>
        /// 傳入ID陣列與權重陣列取得ID陣列索引
        /// </summary>
        public static int WeightIndexGetter(int[] _ids, int[] _weigths) {
            if (_ids.Length != _weigths.Length) {
                WriteLog.Log("傳入計算器的ID陣列與權重陣列不一致");
                return 0;
            }
            int index = 0;
            int sumWeight = 0;
            for (int i = 0; i < _weigths.Length; i++) {
                sumWeight += _weigths[i];
            }
            int randWeight = Random.Range(0, sumWeight);
            for (int i = 0; i < _ids.Length; i++) {
                randWeight -= _weigths[i];
                if (randWeight < 0) {
                    index = i;
                    break;
                }
            }
            return index;
        }
        /// <summary>
        /// 傳入ID陣列與權重陣列取得ID
        /// </summary>
        public static int WeightIDGetter(int[] _ids, int[] _weigths) {
            if (_ids.Length != _weigths.Length) {
                WriteLog.Log("傳入計算器的ID陣列與權重陣列不一致");
                return 0;
            }
            int index = 0;
            int sumWeight = 0;
            for (int i = 0; i < _weigths.Length; i++) {
                sumWeight += _weigths[i];
            }
            int randWeight = Random.Range(0, sumWeight);
            for (int i = 0; i < _ids.Length; i++) {
                randWeight -= _weigths[i];
                if (randWeight < 0) {
                    index = i;
                    break;
                }
            }
            return _ids[index];
        }


        public static int GetOneOrNegativeOne() {
            int r = Random.Range(0, 2);
            return (r == 0) ? 1 : -1;
        }
        public static bool GetTrueOrFalse() {
            int r = Random.Range(0, 2);
            return (r == 0);
        }
        public static List<T> GetRandomTListFromTListByWeights<T>(List<T> _itemList, List<int> _weightList, int _count) {
            if (_itemList == null || _weightList == null)
                return null;
            if (_itemList.Count != _weightList.Count)
                return null;
            if (_count == 0)
                return null;

            List<T> list = new List<T>();
            for (int i = 0; i < _count; i++) {
                T t = _itemList[GetIndexFromWeigth(_weightList)];
                list.Add(t);
            }
            return list;
        }
        public static int GetRandomIndexFromTArray<T>(T[] _array) {
            if (_array == null || _array.Length == 0)
                return 0;
            int rand = Random.Range(0, _array.Length);
            return rand;
        }
        public static int GetRandomIndexFromTList<T>(List<T> _list) {
            if (_list == null || _list.Count == 0)
                return 0;
            int rand = Random.Range(0, _list.Count);
            return rand;
        }
        public static T GetRandomTFromTArray<T>(T[] _array) {
            if (_array == null || _array.Length == 0)
                return default(T);
            int rand = Random.Range(0, _array.Length);
            return _array[rand];
        }
        public static T GetRandomTFromTList<T>(List<T> _list) {
            if (_list == null || _list.Count == 0)
                return default(T);
            int rand = Random.Range(0, _list.Count);
            return _list[rand];
        }
        public static List<T> GetRandomTFromTList<T>(List<T> _list, int _count) {
            if (_list == null || _list.Count == 0 || _count <= 0)
                return null;
            List<T> list = new List<T>();
            for (int i = 0; i < _count; i++) {
                int rand = Random.Range(0, _list.Count);
                list.Add(_list[rand]);
            }
            return list;
        }
        public static T GetRandomTFromTHashSet<T>(HashSet<T> _hashSet) {
            if (_hashSet == null || _hashSet.Count == 0)
                return default(T);
            int rand = Random.Range(0, _hashSet.Count);
            return _hashSet.ToArray()[rand];
        }

        public static List<T> GetRandNoDuplicatedTFromTList<T>(List<T> _itemList, int _count) {
            if (_itemList == null || _itemList.Count == 0) {
                WriteLog.LogError("傳入List錯誤");
                return null;
            }


            if (_count > _itemList.Count) {
                WriteLog.LogError("取的數量不可小於List");
                return null;
            } else if (_count == _itemList.Count) {
                return _itemList;
            }

            List<T> list = new List<T>();
            for (int i = 0; i < _count; i++) {
                int index = GetRandomIndexFromTList(_itemList);
                list.Add(_itemList[index]);
                _itemList.RemoveAt(index);
            }
            return list;
        }

        /// <summary>
        /// 傳入json {"Encounter":3, "Monster":2, "Rest":1}的JSONNode 根據權重隨機取得key值
        /// </summary>
        public static string GetRandomKeyFromJsNodeKeyWeight(JSONNode _jsNode) {
            if (_jsNode == null || _jsNode.Count == 0) {
                WriteLog.LogError("GetRandomTFromJsNode傳入為空");
                return null;
            }
            int totalWeight = 0;
            Dictionary<string, int> weights = new Dictionary<string, int>();

            foreach (KeyValuePair<string, JSONNode> entry in _jsNode) {
                int weight = entry.Value.AsInt;
                totalWeight += weight;
                weights.Add(entry.Key, weight);
            }

            int randomWeight = Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (KeyValuePair<string, int> entry in weights) {
                currentWeight += entry.Value;
                if (randomWeight < currentWeight) {
                    return entry.Key;
                }
            }

            return null;
        }

    }
}
