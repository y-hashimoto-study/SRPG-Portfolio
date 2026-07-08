using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData",menuName = "SRPG/Item")]
public class ItemData : ItemBase
{
    [SerializeField] private ItemEffect _effect;
    public ItemEffect Effect => _effect;
    [SerializeField] private bool _targetSelect;
    public bool TargetSelect => _targetSelect;
    [SerializeField] private int _minTargetRange = 1;
    [SerializeField] private int _maxTargetRange = 1;
    public int MinTargetRange => _minTargetRange;
    public int MaxTargetRange => _maxTargetRange;
}
