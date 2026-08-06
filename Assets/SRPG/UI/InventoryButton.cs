using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System;
public class InventoryButton : MonoBehaviour
{
   [SerializeField] private Image _image;
   [SerializeField]private TextMeshProUGUI _name;
   [SerializeField]private Button _button;
   private ItemBase _item;
   private Action<ItemBase> _customClickAction;
   public void Awake()
    {
        _button.onClick.AddListener(ClickButton);
    }
   public void SetUp(ItemBase item,bool equipped,Action<ItemBase> customClickAction = null)
    {
        _item = item;
        _customClickAction = customClickAction;
        if(_item == null)return;
        _image.sprite = _item.Icon;
        string setText = (equipped) ? "(E)" + _item.Name : _item.Name;
        _name.text = setText;
    }
    public void ClickButton()
    {
        if(_item != null)
        {
            if(_customClickAction != null)
            {
                _customClickAction.Invoke(_item);
            }
            else
            {
                _item.ClickInventory();
            }
        }
    }
    public void EquippedWeapon(bool equipped)
    {
        if(_item == null)return;
        string setText = (equipped) ? "(E)" + _item.Name : _item.Name;
        _name.text = setText;
    }
    void OnDisable()
    {
        _customClickAction = null;
    }
}
