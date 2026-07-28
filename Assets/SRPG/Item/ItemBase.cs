using UnityEngine;

public abstract class ItemBase : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    public abstract void ClickInventory();
}
