using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace Scoz.Func
{
    public class PropogateDrag : MonoBehaviour
    {
        ScrollRect MyScrollView;
        // Start is called before the first frame update
        public void UpdateScrollEvent(ScrollRect _scrollRect)
        {
            MyScrollView = _scrollRect;
            EventTrigger trigger = GetComponent<EventTrigger>();
            EventTrigger.Entry entryBegin = new EventTrigger.Entry(), entryDrag = new EventTrigger.Entry(), entryEnd = new EventTrigger.Entry(), entrypotential = new EventTrigger.Entry()
                , entryScroll = new EventTrigger.Entry();

            entryBegin.eventID = EventTriggerType.BeginDrag;
            entryBegin.callback.AddListener((data) => { MyScrollView.OnBeginDrag((PointerEventData)data); });
            trigger.triggers.Add(entryBegin);

            entryDrag.eventID = EventTriggerType.Drag;
            entryDrag.callback.AddListener((data) => { MyScrollView.OnDrag((PointerEventData)data); });
            trigger.triggers.Add(entryDrag);

            entryEnd.eventID = EventTriggerType.EndDrag;
            entryEnd.callback.AddListener((data) => { MyScrollView.OnEndDrag((PointerEventData)data); });
            trigger.triggers.Add(entryEnd);

            entrypotential.eventID = EventTriggerType.InitializePotentialDrag;
            entrypotential.callback.AddListener((data) => { MyScrollView.OnInitializePotentialDrag((PointerEventData)data); });
            trigger.triggers.Add(entrypotential);

            entryScroll.eventID = EventTriggerType.Scroll;
            entryScroll.callback.AddListener((data) => { MyScrollView.OnScroll((PointerEventData)data); });
            trigger.triggers.Add(entryScroll);
        }
    }
}