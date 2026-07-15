using UnityEngine;
using System.Collections.Generic;
public abstract class UnitBase : MonoBehaviour,IMapObject
{
  public enum Type { Player, Enemy, Gimmick }
  [SerializeField] protected Type _team;
  public Type Team => _team;
  public IMapObject CurrentObject { get; set; }
  public UnitBase CurrentUnit => CurrentObject as UnitBase;
  [SerializeField] protected UnitStatus _baseStatus;
  [SerializeField] private string _name;
  public string Name => _name;
  protected int _hp;
  public int Hp => _hp;
  public int MaxHp => _baseStatus.MaxHp;
  public int Atk => _baseStatus.Atk + ((_equipedWeapon == null) ? 0:(_equipedWeapon.IsMagic)? 0:_equipedWeapon.Power);
  public int Def => _baseStatus.Def;
  public int MAtk => _baseStatus.MAtk + ((_equipedWeapon == null) ? 0:(_equipedWeapon.IsMagic)? _equipedWeapon.Power:0);
  public int MDef => _baseStatus.MDef;
  public int Spd => _baseStatus.Spd;
  public int Mov => _baseStatus.Mov;
  public Vector2Int Position { get; set; }
  public UnityEngine.GameObject GameObject => this.gameObject;

  protected bool _isActed = false;
  public bool IsActed => _isActed;
  protected Color _originalColor;
  private Material _unitMaterial;
  public bool IsAttackable => true;

  private WeaponData _equipedWeapon = null;
  public WeaponData EquipedWeapon => _equipedWeapon;
  public List<ItemBase> Inventory = new List<ItemBase>();
  public int MaxBagSize => (_equipedWeapon == null) ? 5:4;
  public bool IsMagic => (_equipedWeapon == null) ? false:_equipedWeapon.IsMagic;
  public int MinAttackRange => (_equipedWeapon == null) ? 1:_equipedWeapon.MinAttackRange;
  public int MaxAttackRange => (_equipedWeapon == null) ? 1:_equipedWeapon.MaxAttackRange;
  protected virtual void Awake()
  {
    _hp = _baseStatus.MaxHp;
    _unitMaterial = GetComponent<MeshRenderer>().material;
    Position = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
    if (_unitMaterial != null) _originalColor = _unitMaterial.color;
  }
  protected virtual void Start()
  {
    MapCube onCube = MapManager.Instance.GetMapCube(Position);
    if (onCube != null)
    {
      onCube.CurrentObject = this;
      Debug.Log($"{gameObject.name} を {Position} に登録しました");
      BattleManager.Instance.AddAllUnitList(this);
    }
  }
  public virtual void Damage(int attack , bool isMagic)
  {
    int armor = (isMagic) ? MDef : Def;
    int damage = (attack - armor > 0) ? attack - armor : 0;
    _hp -= damage;
    Debug.Log($"{gameObject.name} は {damage} のダメージを受けた！ (残りHP: {_hp})");
    //演出ここにかく？
    if (_hp <= 0)
    {
      Die();
    }
  }
  public virtual void HealHp(int healValue)
  {
    _hp = (MaxHp > _hp + healValue) ? _hp + healValue : MaxHp;
    Debug.Log($"{gameObject.name} は {healValue} 回復した！ (残りHP: {_hp})");
  }
  public virtual void MoveFinish()
  {
    _isActed = true;
    _unitMaterial.color = Color.black;
  }
  public virtual void MoveReset()
  {
    _isActed = false;
    _unitMaterial.color = _originalColor;
  }
  private void OnDestroy()
  {
    if (_unitMaterial != null)
    {
      Destroy(_unitMaterial);
    }
  }
  public virtual void Die()
  {
    BattleManager.Instance.DieUnit(Position);
    Destroy(gameObject);
  }
  public void AddItem(ItemBase getItem)
  {
    if(Inventory.Count >= MaxBagSize) return;//入れ替えるイベントを作っても良い
    Inventory.Add(getItem);
  }
  public void Equiped(WeaponData weapon)
  {
    _equipedWeapon = weapon;
  }
}
