using UnityEngine;
using System.Collections;
public interface IMapObject
{
    Vector2Int Position { get; set; }
    UnitBase.Type Team { get; }
    UnityEngine.GameObject GameObject { get; }
    bool IsAttackable { get; }
    string Name { get; }
    public int Damage(int attack , bool isMagic);
    void CheckDie();
}
