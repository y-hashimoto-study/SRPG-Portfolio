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
   public Action<ItemBase> ItemButtonClicked;
   public void Awake()
    {
        _button.onClick.AddListener(ClickButton);
    }
   public void SetUp(ItemBase item,bool equipped)
    {
        _item = item;
        if(_item == null)return;
        _image.sprite = _item.Icon;
        string setText = (equipped) ? "(E)" + _item.Name : _item.Name;
        _name.text = setText;
    }
    public void ClickButton()
    {
        ItemButtonClicked?.Invoke(_item);
    }
    public void EquippedWeapon(bool equipped)
    {
        if(_item == null)return;
        string setText = (equipped) ? "(E)" + _item.Name : _item.Name;
        _name.text = setText;
    }
}
