using UnityEngine;
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
    public void OpenInventory(UnitBase unit)
    {
        int i = 0;
        if(unit is PlayerUnit player)
        {
            if(player.EquipedWeapon != null)
            {
                WeaponData weapon = player.EquipedWeapon;
                if(i >= _inventoryButton.Count)return;
                _inventoryButton[i].gameObject.SetActive(true);
                _inventoryButton[i].SetUp(player.EquipedWeapon,true);

                _inventoryButton[i].ItemButtonClicked -= BattleManager.Instance.Equiped;
                _inventoryButton[i].ItemButtonClicked -= BattleManager.Instance.SetUseItem;
                _inventoryButton[i].ItemButtonClicked += BattleManager.Instance.Equiped;
                i++;
            }
            foreach (ItemBase item in player.Inventory)
            {
                if(i >= _inventoryButton.Count)return;
                _inventoryButton[i].gameObject.SetActive(true);
                _inventoryButton[i].SetUp(item,false);

                _inventoryButton[i].ItemButtonClicked -= BattleManager.Instance.Equiped;
                _inventoryButton[i].ItemButtonClicked -= BattleManager.Instance.SetUseItem;
                _inventoryButton[i].ItemButtonClicked += BattleManager.Instance.SetUseItem;
                i++;
            }
            while(i < _inventoryButton.Count)
            {
                _inventoryButton[i].gameObject.SetActive(false);
                i++;
            }
        }
    }
    void OnDisable()
    {
        foreach(InventoryButton button in _inventoryButton)
        {
            button.ItemButtonClicked -= BattleManager.Instance.Equiped;
            button.ItemButtonClicked -= BattleManager.Instance.SetUseItem;
        }
    }
}
