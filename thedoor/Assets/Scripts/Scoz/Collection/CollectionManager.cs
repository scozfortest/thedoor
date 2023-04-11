using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scoz.Func {
    public static class CollectionManager {
        public static void RemoveAt<T>(ref T[] _arr, int _index) {
            for (int i = _index; i < _arr.Length - 1; i++) {
                _arr[i] = _arr[i + 1];
            }
            System.Array.Resize(ref _arr, _arr.Length - 1);
        }
        public static void AddRange<T>(this ICollection<T> _target, IEnumerable<T> _source) {
            if (_target == null)
                throw new System.ArgumentNullException(nameof(_target));
            if (_source == null)
                throw new System.ArgumentNullException(nameof(_source));
            foreach (var element in _source)
                _target.Add(element);
        }
        public static List<T> GetReArrangeListWithOffset<T>(List<T> _list, int _offset) {
            List<T> result = new List<T>();
            _offset = _offset % _list.Count;
            if (_offset == 0)
                return _list;
            for (int i = 0; i < _list.Count; i++) {
                int index = i + _offset;
                if (index >= _list.Count)
                    index -= _list.Count;
                else if (index < 0)
                    index = _list.Count + index;
                result.Add(_list[index]);
            }
            return result;
        }
        public static T GetRandomTFromList<T>(List<T> _list) {
            if (_list == null || _list.Count <= 0)
                return default(T);
            int rand = Random.Range(0, _list.Count);
            return _list[rand];
        }
        public static List<T> GetRepeatTListyFromList<T>(List<T> _list, int _count) {
            if (_list == null || _list.Count <= 0 || _count > _list.Count)
                return default(List<T>);
            if (_count == _list.Count)
                return _list;
            List<T> result = new List<T>();
            for (int i = 0; i < _count; i++) {
                int index = Random.Range(0, _list.Count);
                result.Add(_list[index]);
            }
            return result;
        }
        public static List<T> GetNoRepeatTListyFromList<T>(List<T> _list, int _count) {
            if (_list == null || _list.Count <= 0 || _count > _list.Count)
                return default(List<T>);
            if (_count == _list.Count)
                return _list;
            List<T> result = new List<T>();
            for (int i = 0; i < _count; i++) {
                int index = Random.Range(0, _list.Count);
                result.Add(_list[index]);
                _list.RemoveAt(index);
            }
            return result;
        }
        public static T GetRandomTFromArray<T>(T[] _array) {
            if (_array == null || _array.Length <= 0)
                return default(T);
            int rand = Random.Range(0, _array.Length);
            return _array[rand];
        }


        public static List<T> AddTIntoTListAtRandomIndex<T>(List<T> _list, T _t) {
            int index = Random.Range(0, _list.Count);
            _list.Insert(index, _t);
            return _list;
        }
        public static List<T> AddTListIntoTListAtRandomIndex<T>(List<T> _list, List<T> _addTList) {
            for (int i = 0; i < _addTList.Count; i++) {
                int index = Random.Range(0, _list.Count);
                _list.Insert(index, _addTList[i]);
            }
            return _list;
        }
        public static void MoveItemToLast<T>(ref List<T> _list, int _index) {
            T item = _list[_index];
            _list.RemoveAt(_index);
            _list.Add(item);
        }
        //傳入List與分割長度，將List依據指定長度切分成多個List
        //EX. 傳入陣列[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22]與10，會回傳Array [Array [1, 2, 3, 4, 5, 6, 7, 8, 9, 10], Array [11, 12, 13, 14, 15, 16, 17, 18, 19, 20], Array [21, 22]]
        public static List<List<T>> SplitList<T>(List<T> _list, int _splitLength) {
            List<List<T>> newList = new List<List<T>>();
            while (_list.Count > _splitLength) {
                newList.Add(_list.GetRange(0, _splitLength));
                _list.RemoveRange(0, _splitLength);
            }
            newList.Add(_list);
            return newList;
        }

    }

}