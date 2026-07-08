using UnityEngine;
using UnityEngine.UI;
using System;
public class Command : MonoBehaviour
{
    [SerializeField] private Button ActionButton;
    [SerializeField] private Button AttackButton;
    [SerializeField] private Button WaitButton;
    [SerializeField] private Button ItemButton;
    [SerializeField] private Button ReturnButton;
    void Awake()
    {
        ActionButton.onClick.AddListener(Debugyou);
        AttackButton.onClick.AddListener(MapManager.Instance.SetAttackMode);
        WaitButton.onClick.AddListener(MapManager.Instance.MoveFinish);
        ItemButton.onClick.AddListener(() =>
        {
            UIManager.Instance.OpenInventory();
            UIManager.Instance.PushMenu(UIManager.MenuUIStateEnum.Inventory);
        });
        
        ReturnButton.onClick.AddListener(UIManager.Instance.BackMenu);
    }
    public void OpenCommand(bool isAction,bool isAttack)
    {
        ActionButton.gameObject.SetActive(isAction);
        AttackButton.gameObject.SetActive(isAttack);
    }
    public void Debugyou()
    {
        Debug.Log("宝箱を開けるなどのイベント");
    }
}
