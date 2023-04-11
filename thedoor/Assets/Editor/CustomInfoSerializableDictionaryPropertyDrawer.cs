using UnityEngine;
using UnityEditor;
using Scoz.Func;

namespace TheDoor.Main {
    [CustomPropertyDrawer(typeof(PostProcessingManager.BloomSettingDicClass))]

    public class CustomInfoSerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
}