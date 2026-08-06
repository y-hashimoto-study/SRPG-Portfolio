using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [SerializeField] private List<InventoryButton> _inventoryButton = new List<InventoryButton>();
    [SerializeField] private Button _backButton;
    void Start()
    {
        _backButton.onClick.AddListener(UIManager.Instance.BackMenu);
    }
    public void OpenInventory(UnitBase unit,Action<ItemBase> ClickAction = null)
    {
        int i = 0;
        if(unit is PlayerUnit player)
        {
            if(player.EquipedWeapon != null)
            {
                WeaponData weapon = player.EquipedWeapon;
                if(i >= _inventoryButton.Count)return;
                _inventoryButton[i].gameObject.SetActive(true);
                _inventoryButton[i].SetUp(weapon,true,
                (item) =>
                {
                    string message = $"{item.Name}は捨てれません";
                    UIManager.Instance.PopUpMessage(message);
                });
                i++;
            }
            foreach (ItemBase item in player.Inventory)
            {
                if(i >= _inventoryButton.Count)return;
                _inventoryButton[i].gameObject.SetActive(true);
                _inventoryButton[i].SetUp(item,false,ClickAction);
                i++;
            }
            while(i < _inventoryButton.Count)
            {
                _inventoryButton[i].gameObject.SetActive(false);
                i++;
            }
        }
    }
}
