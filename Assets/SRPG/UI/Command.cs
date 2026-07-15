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
    void Start()
    {
        ActionButton.onClick.AddListener(ToDOACtion);
        AttackButton.onClick.AddListener(BattleManager.Instance.SetAttackTargetMode);
        WaitButton.onClick.AddListener(BattleManager.Instance.MoveFinish);
        ItemButton.onClick.AddListener(() =>
        {
            UIManager.Instance.OpenInventory(BattleManager.Instance.SelectedUnit);
            UIManager.Instance.PushMenu(UIManager.MenuUIStateEnum.Inventory);
        });
        
        ReturnButton.onClick.AddListener(UIManager.Instance.BackMenu);
    }
    public void OpenCommand(bool isAction,bool isAttack)
    {
        ActionButton.gameObject.SetActive(isAction);
        AttackButton.gameObject.SetActive(isAttack);
    }
    public void ToDOACtion()
    {
        Debug.Log("宝箱を開けるなどのイベント");
    }
}
