using UnityEngine;

public interface IMapObject
{
    Vector2Int Position { get; set; }
    UnitBase.Type Team { get; }
    UnityEngine.GameObject GameObject { get; }
    bool IsAttackable { get; }
    string Name { get; }
    void Damage(int attack , bool isMagic);
}
