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
    public void OpenInventory()
    {
        int i = 0;
        if(MapManager.Instance.SelectedUnit is PlayerUnit player)
        {
            if(player.EquipedWeapon != null)
            {
                if(i >= _inventoryButton.Count)return;
                _inventoryButton[i].gameObject.SetActive(true);
                _inventoryButton[i].SetUp(player.EquipedWeapon,true);

                _inventoryButton[i].ItemButtonClicked -= MapManager.Instance.Equiped;
                _inventoryButton[i].ItemButtonClicked -= MapManager.Instance.UseItem;
                _inventoryButton[i].ItemButtonClicked += MapManager.Instance.Equiped;
                i++;
            }
            foreach (ItemBase item in player.Inventory)
            {
                if(i >= _inventoryButton.Count)return;
                _inventoryButton[i].gameObject.SetActive(true);
                _inventoryButton[i].SetUp(item,false);

                _inventoryButton[i].ItemButtonClicked -= MapManager.Instance.Equiped;
                _inventoryButton[i].ItemButtonClicked -= MapManager.Instance.UseItem;
                _inventoryButton[i].ItemButtonClicked += MapManager.Instance.UseItem;
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
            button.ItemButtonClicked -= MapManager.Instance.Equiped;
            button.ItemButtonClicked -= MapManager.Instance.UseItem;
        }
    }
}
