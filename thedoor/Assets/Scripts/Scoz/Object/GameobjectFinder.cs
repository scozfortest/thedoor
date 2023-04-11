using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scoz.Func
{
    public class GameobjectFinder : MonoBehaviour
    {
        public static GameObject SelfGameobject;
        public static GameObject FindClosestGameobjectWithTags(GameObject _self, params string[] _tags)
        {
            List<GameObject> goList = new List<GameObject>();
            for(int i=0;i< _tags.Length;i++)
            {
                goList.AddRange(GameObject.FindGameObjectsWithTag(_tags[i]));
            }
            GameObject closest = null;
            float distance = Mathf.Infinity;
            SelfGameobject = _self;
            Vector3 position = SelfGameobject.transform.position;
            foreach (GameObject go in goList)
            {
                if (go.GetInstanceID() == _self.gameObject.GetInstanceID())
                    continue;
                float curDistance = Vector2.Distance(go.transform.position, position);
                if (curDistance < distance)
                {
                    closest = go;
                    distance = curDistance;
                }
            }
            return closest;
        }
        public static GameObject FindInRangeClosestGameobjectWithTag(GameObject _self, int _range, params string[] _tags)
        {
            List<GameObject> goList = new List<GameObject>();
            for (int i = 0; i < _tags.Length; i++)
            {
                goList.AddRange(GameObject.FindGameObjectsWithTag(_tags[i]));
            }
            GameObject closest = null;
            float distance = Mathf.Infinity;
            SelfGameobject = _self;
            Vector3 position = SelfGameobject.transform.position;
            foreach (GameObject go in goList)
            {
                if (go.GetInstanceID() == _self.gameObject.GetInstanceID())
                    continue;
                float curDistance = Vector2.Distance(go.transform.position, position);
                if (curDistance > _range)
                    continue;
                if (curDistance < distance)
                {
                    closest = go;
                    distance = curDistance;
                }
            }
            return closest;
        }
        public static List<GameObject> FindClosestSpecificNumberOfGameobjectsWithTags(GameObject _self, int _count, params string[] _tags)
        {
            List<GameObject> goList = new List<GameObject>();
            for (int i = 0; i < _tags.Length; i++)
            {
                goList.AddRange(GameObject.FindGameObjectsWithTag(_tags[i]));
            }
            SelfGameobject = _self;
            goList.Remove(SelfGameobject);
            goList.Sort(SortByDistance);
            List<GameObject> targetList = new List<GameObject>();
            for (int i = 0; i < _count; i++)
            {
                if (i < goList.Count)
                    targetList.Add(goList[i]);
                else
                    break;
            }
            return targetList;
        }
        public static List<GameObject> FindClosestSpecificNumberOfGameobjectsInRangeWithTags(GameObject _self, int _count, int _range,params string[] _tags )
        {
            List<GameObject> goList = new List<GameObject>();
            for (int i = 0; i < _tags.Length; i++)
            {
                goList.AddRange(GameObject.FindGameObjectsWithTag(_tags[i]));
            }
            SelfGameobject = _self;
            Vector3 position = SelfGameobject.transform.position;
            goList.Remove(SelfGameobject);
            goList.Sort(SortByDistance);
            List<GameObject> targetList = new List<GameObject>();
            for (int i = 0; i < _count; i++)
            {
                if (i < goList.Count)
                {
  
                    float curDistance = Vector2.Distance(goList[i].transform.position, position);
                    if (curDistance > _range)
                        break;
                    else
                        targetList.Add(goList[i]);
                }
                else
                    break;
            }
            return targetList;
        }
        public static List<GameObject> FindClosestSpecificNumberOfNotTGameobjectsInRangeWithTags<T>(GameObject _self, int _count, int _range, params string[] _tags)
        {
            List<GameObject> goList = new List<GameObject>();
            for (int i = 0; i < _tags.Length; i++)
            {
                goList.AddRange(GameObject.FindGameObjectsWithTag(_tags[i]));
            }
            SelfGameobject = _self;
            Vector3 position = SelfGameobject.transform.position;
            goList.Remove(SelfGameobject);
            goList.RemoveAll(item => item.GetComponent<T>() != null);
            if (goList.Count > 0)
            {
                goList.Sort(SortByDistance);
                List<GameObject> targetList = new List<GameObject>();
                for (int i = 0; i < _count; i++)
                {
                    if (i < goList.Count)
                    {
                        float curDistance = Vector2.Distance(goList[i].transform.position, position);
                        if (curDistance > _range)
                            break;
                        else
                            targetList.Add(goList[i]);
                    }
                    else
                        break;
                }
                return targetList;
            }
            return null;
        }
        public static List<GameObject> FindInRangeGameobjectsWithTag(GameObject _self, int _range, params string[] _tags)
        {
            List<GameObject> goList = new List<GameObject>();
            List<GameObject> inRangeGoList = new List<GameObject>();
            for (int i = 0; i < _tags.Length; i++)
            {
                goList.AddRange(GameObject.FindGameObjectsWithTag(_tags[i]));
            }
            SelfGameobject = _self;
            Vector3 position = SelfGameobject.transform.position;
            foreach (GameObject go in goList)
            {
                if (go.GetInstanceID() == _self.gameObject.GetInstanceID())
                    continue;
                float curDistance = Vector2.Distance(go.transform.position, position);
                if (curDistance > _range)
                    continue;
                inRangeGoList.Add(go);
            }
            return inRangeGoList;
        }
        public static List<GameObject> FindGameobjectsWithTag(GameObject _self, params string[] _tags)
        {
            List<GameObject> goList = new List<GameObject>();
            for (int i = 0; i < _tags.Length; i++)
            {
                goList.AddRange(GameObject.FindGameObjectsWithTag(_tags[i]));
            }
            foreach (GameObject go in goList)
            {
                if (go.GetInstanceID() == _self.gameObject.GetInstanceID())
                {
                    goList.Remove(go);
                    break;
                }
            }
            return goList;
        }
        public static List<GameObject> FindGameobjectsWithTags( params string[] _tags)
        {
            List<GameObject> goList = new List<GameObject>();
            for (int i = 0; i < _tags.Length; i++)
            {
                goList.AddRange(GameObject.FindGameObjectsWithTag(_tags[i]));
            }
            return goList;
        }
        public static int SortByDistance(GameObject _go1, GameObject _go2)
        {
            float dstToA = Vector3.Distance(SelfGameobject.transform.position, _go1.transform.position);
            float dstToB = Vector3.Distance(SelfGameobject.transform.position, _go2.transform.position);
            return dstToA.CompareTo(dstToB);
        }
        public static Transform FindAllChildren(Transform _parent,string _name)
        {
            Transform[] allChildren = _parent.GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildren)
            {
                if (child.name == _name)
                {
                    return child;
                }
            }
            return null;
        }
        /// <summary>
        /// 找出某個Transfrom底下不包含指定子層底下的所有Renderer(耗效能)
        /// </summary>
        public static List<Renderer> FindAllRendererInChildrenExceptUnderParentNames(Transform _parent, params string[] _exceptParentNames)
        {
            if (_exceptParentNames == null || _exceptParentNames.Length == 0)
                return null;
            Renderer[] renderers = _parent.GetComponentsInChildren<Renderer>();
            List<Renderer> availableRenderers = new List<Renderer>();
            for(int i=0;i< renderers.Length;i++)
            {
                bool available = true;
                Transform tmpTrans = renderers[i].transform;
                if (!MyCompare.EqualToOneOfAll(tmpTrans.name, _exceptParentNames))
                {
                    while (tmpTrans.parent != null)
                    {
                        if (MyCompare.EqualToOneOfAll(tmpTrans.parent.name, _exceptParentNames))
                        {
                            available = false;
                            break;
                        }
                        tmpTrans = tmpTrans.parent;
                    }
                }
                else
                    available = false;


                if (available)
                    availableRenderers.Add(renderers[i]);
            }
            return availableRenderers;
        }
        public static List<Renderer> FindAllRendererInChildrenExceptParentNames(Transform _parent,params string[] _exceptParentNames)
        {
            if (_exceptParentNames == null || _exceptParentNames.Length == 0)
                return null;
            Renderer[] allChildren = _parent.GetComponentsInChildren<Renderer>();
            List<Renderer> list = new List<Renderer>();
            foreach (Renderer child in allChildren)
            {
                for(int i=0;i< _exceptParentNames.Length;i++)
                {
                    DebugLogger.Log(child.transform.parent.name + ":" + _exceptParentNames[i]);
                    if (child.transform.parent.name != _exceptParentNames[i])
                        list.Add(child);
                }
            }
            return list;
        }
    }
}
