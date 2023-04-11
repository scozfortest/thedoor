using UnityEngine;
using UnityEngine.UI;
using Scoz.Func;
public sealed class ContentSizeSum : MonoBehaviour
{
    [SerializeField]
    RectTransform[] Rects = null;
    private void Start()
    {
        UpdateSize();
    }
    public void UpdateSize()
    {
        float height = 0;

        for (int i = 0; i < Rects.Length; i++)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Rects[i]);
            height += Rects[i].sizeDelta.y;
            DebugLogger.Log(Rects[i].name+ "="+Rects[i].sizeDelta.y);
        }
        RectTransform rt = gameObject.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
    }
}
