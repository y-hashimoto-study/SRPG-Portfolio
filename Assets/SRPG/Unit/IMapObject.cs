using UnityEngine;
using System.Collections;
public interface IMapObject
{
    Vector2Int Position { get; set; }
    UnitBase.Type Team { get; }
    UnityEngine.GameObject GameObject { get; }
    bool IsAttackable { get; }
    string Name { get; }
    IEnumerator Damage(int attack , bool isMagic);
}
