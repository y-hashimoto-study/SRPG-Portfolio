using UnityEngine;

public class GimmickBase : MonoBehaviour,IMapObject
{
    public UnityEngine.GameObject GameObject => this.gameObject;
    public Vector2Int Position { get; set; }
    public UnitBase.Type Team => UnitBase.Type.Gimmick;
    [SerializeField] private bool _isAttackable = true;
    public bool IsAttackable => _isAttackable;
    public IMapObject CurrentObject { get; set; }
    public UnitBase CurrentUnit => CurrentObject as UnitBase;
    public GimmickBase CurrentGimmick => CurrentObject as GimmickBase;
    public bool IsActionable;
    [SerializeField] private string _name;
    public string Name => _name;
    [SerializeField]private int _hp;
    public int Hp => _hp;
    [SerializeField]private int _maxhp;
    public int MaxHp => _maxhp;
    public void Damage(int attack,bool isMagic)
    {
        
    }
}
