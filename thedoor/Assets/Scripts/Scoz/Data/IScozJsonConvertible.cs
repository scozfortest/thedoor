using SimpleJSON;

/// <summary>
/// 繼承IScozJsonConvertible的類別可以使用ToScozJson 轉換的對象是有標記為ScozSerializable的屬性
/// 限制: 如果迭代類屬性只能是List, Dictionary, array
/// </summary>
public interface IScozJsonConvertible {
}