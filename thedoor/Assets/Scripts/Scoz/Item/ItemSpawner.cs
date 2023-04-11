using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scoz.Func
{
    public class ItemSpawner<T> : BaseUI where T : MonoBehaviour, IItem
    {
        public T ItemPrefab;
        public Transform ParentTrans = null;
        [HideInInspector]
        public List<T> ItemList = new List<T>();


        public T Spawn<V>() where V : T
        {
            V item = Instantiate(ItemPrefab) as V;
            item.transform.SetParent(ParentTrans);
            item.transform.localPosition = Vector3.zero;
            item.transform.localScale = ItemPrefab.transform.localScale;
            ItemList.Add(item);
            return item;
        }

        public void ClearItems()

        {
            for (int i = 0; i < ItemList.Count; i++)
            {
                Destroy(ItemList[i].gameObject);
            }
            ItemList.Clear();
        }
    }
}
