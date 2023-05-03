using System;
namespace Scoz.Func {
    [AttributeUsage(AttributeTargets.Property)]
    public class ScozSerializableAttribute : Attribute {
        //標記為ScozJsonSerializableAttribute的屬性可透過ScozJsonConverter將繼承IScozJsonConvertible介面的class轉為json
    }
}