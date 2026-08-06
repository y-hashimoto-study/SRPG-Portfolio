using UnityEngine;

public class PlayerUnit : UnitBase
{
    public void AddItem(ItemBase getItem)
    {
        if(Inventory.Count >= MaxBagSize) return;//一旦
        Inventory.Add(getItem);
    }
    public void Equiped(WeaponData weapon)
    {
        if(_equipedWeapon == null)
        {
            _equipedWeapon = weapon;
            Inventory.Remove(weapon);
        }
        else if(weapon != _equipedWeapon)
        {
            WeaponData beforeweapon = _equipedWeapon;
            _equipedWeapon = weapon;
            Inventory.Remove(weapon);
            Inventory.Add(beforeweapon);
        }
        else
        {
            Inventory.Add(_equipedWeapon);
            _equipedWeapon = null;
        }
    }
    public bool CheckInventoryFull()
    {
        bool isFull = MaxBagSize <= Inventory.Count;
        return isFull;
    }
}
