using UnityEngine;
[CreateAssetMenu(fileName = "NewDamage",menuName = "SRPG/Item/Effect/Damage")]
public class DamageEffect : ItemEffect
{
    [SerializeField] private int _damageValue;
    [SerializeField] private bool _isMagic;
    public override void UseItem(UnitBase target)
    {
        target.Damage(_damageValue,_isMagic);
    }
    public override (bool,string) CanUse(UnitBase user,UnitBase target)
    {
        if(target == null)return (false,null);
        if(user.Team == target.Team) return (false,null);
        return (true,null);
    }
}
