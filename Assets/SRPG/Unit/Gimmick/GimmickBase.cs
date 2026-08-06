using UnityEngine;
using System.Collections;

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
    [SerializeField] private string _name;
    public string Name => _name;
    [SerializeField]private int _hp;
    public int Hp => _hp;
    [SerializeField]private int _maxhp;
    public int MaxHp => _maxhp;
    protected virtual void Awake()
    {
        _hp = _maxhp;
        Position = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
    }
    protected virtual void Start()
    {
        MapCube onCube = MapManager.Instance.GetMapCube(Position);
        if(onCube != null)
        {
            onCube.CurrentObject = this;
            Debug.Log($"{gameObject.name} を {Position} に登録しました");
        }
    }
    public virtual int Damage(int attack,bool isMagic)
    {
        if(_isAttackable == false) return 0;   
        _hp -= attack;
        Debug.Log($"{gameObject.name} は {attack} のダメージを受けた！ (残りHP: {_hp})");

        if (_hp <= 0)
        {
            Debug.Log("破壊時のイベント");
        }
        return attack;
    }
    public virtual void HealHp(int healValue)
    {

    }
    public virtual void CheckDie()
    {
        //オブジェクトが破壊された時のイベント
    }
}
