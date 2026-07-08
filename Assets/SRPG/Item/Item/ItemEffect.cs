using UnityEngine;

public abstract class ItemEffect : ScriptableObject
{
    public abstract void UseItem(UnitBase target);
    public virtual (bool,string) CanUse(UnitBase user,UnitBase target)
    {
        return (true,null);
    }
}
