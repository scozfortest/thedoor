using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scoz.Func
{
    public class ScrollMap2D : MonoBehaviour
    {
        public float ScrollSpeed = 1;
        public Transform Parent;
        public List<SpriteRenderer> SpriteList = new List<SpriteRenderer>();
        public List<Vector3> PosList = new List<Vector3>();
        Vector3 InitialPos;
        float MapSpacing;
        int CurMapIndexOffset = 0;
        bool Scrolling = false;
        public enum ArrangeMode
        {
            HeadToTailRepeat,
        }
        public void SpawnSprites(List<Sprite> _list)
        {
            if (_list == null || _list.Count < 0)
                return;
            InitialPos = Parent.localPosition;
            MapSpacing = _list[0].rect.width / _list[0].pixelsPerUnit;
            for (int i = 0; i < _list.Count; i++)
            {
                GameObject go = new GameObject();
                SpriteList.Add(go.AddComponent<SpriteRenderer>());
                SpriteList[i].sprite = _list[i];
                go.transform.SetParent(Parent);
                PosList.Add(new Vector3(0 + i * MapSpacing, 0));
            }
            Arrange(CurMapIndexOffset);
        }

        public void Arrange(int _offset)
        {
            List<Vector3> tmpPosList = CollectionManager.GetReArrangeListWithOffset(PosList, _offset);
            for (int i = 0; i < SpriteList.Count; i++)
            {
                SpriteList[i].transform.localPosition = tmpPosList[i];
            }
        }
        public void StartScroll()
        {
            Scrolling = true;
        }
        public void StopScroll()
        {
            Scrolling = false;
        }
        void FixedUpdate()
        {
            if (Scrolling)
            {
                Parent.localPosition -= new Vector3(ScrollSpeed, 0, 0);
                if (Parent.localPosition.x <= (InitialPos.x - MapSpacing))
                {
                    CurMapIndexOffset--;
                    Arrange(CurMapIndexOffset);
                    Parent.localPosition = InitialPos;
                }
            }
        }
    }
}
