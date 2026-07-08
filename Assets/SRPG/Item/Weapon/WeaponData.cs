using UnityEngine;
[CreateAssetMenu(fileName = "NewWeaponData",menuName = "SRPG/Weapon")]
public class WeaponData : ItemBase
{
    private string _explanation;
    [SerializeField] private int _power;
    [SerializeField] private bool _isMagic;
    [SerializeField] private int _minAttackRange;
    [SerializeField] private int _maxAttackRange;

    public string Explanation => _explanation;
    public int Power => _power;
    public bool IsMagic => _isMagic;
    public int MinAttackRange => _minAttackRange;
    public int MaxAttackRange => _maxAttackRange;
}
