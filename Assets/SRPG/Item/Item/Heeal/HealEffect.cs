using UnityEngine;
[CreateAssetMenu(fileName = "NewHeal",menuName = "SRPG/Item/Effect/Heal")]
public class HealEffect : ItemEffect
{
    [SerializeField] private int _healValue;
    public override void UseItem(UnitBase target)
    {
        target.HealHp(_healValue);
    }
    public override (bool,string) CanUse(UnitBase user,UnitBase target)
    {
        (bool,string) canUse = (target.Hp == target.MaxHp) ? (false,"体力が満タンです") : (true,null);
        return canUse;
    }  
}

