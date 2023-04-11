using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineManager : MonoBehaviour {
    public enum LineMsgType {
        text,
        image,
    }
    public static void LineShare(LineMsgType _type,string _content ) {
        Application.OpenURL(string.Format("line://msg/{0}/{1}", _type.ToString(), _content));
    }
}
