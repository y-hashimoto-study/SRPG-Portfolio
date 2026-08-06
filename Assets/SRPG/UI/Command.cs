using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro; 
public class Command : MonoBehaviour
{
    [SerializeField] private Button ActionButton;
    [SerializeField] private TextMeshProUGUI ActionButtonText;
    private IActionable _targetGimmick;
    [SerializeField] private Button AttackButton;
    [SerializeField] private Button WaitButton;
    [SerializeField] private Button ItemButton;
    [SerializeField] private Button ReturnButton;
    void Start()
    {
        ActionButton.onClick.AddListener(() =>
        {
            UIManager.Instance.CloseCommand();
            BattleManager.Instance.ActionGimmick(_targetGimmick);
        });
        AttackButton.onClick.AddListener(BattleManager.Instance.SetAttackTargetMode);
        WaitButton.onClick.AddListener(BattleManager.Instance.MoveFinish);
        ItemButton.onClick.AddListener(() =>
        {
            UIManager.Instance.OpenInventory(BattleManager.Instance.SelectedUnit);
            UIManager.Instance.PushMenu(UIManager.MenuUIStateEnum.Inventory);
        });
        
        ReturnButton.onClick.AddListener(UIManager.Instance.BackMenu);
    }
    public void OpenCommand(IActionable actionable,bool isAttack)
    {
        if(actionable == null)
        {
            ActionButton.gameObject.SetActive(false);
        }
        else
        {
            _targetGimmick = actionable;
            ActionButtonText.text = actionable.ActionName;
            ActionButton.gameObject.SetActive(true);
        }
        AttackButton.gameObject.SetActive(isAttack);
    }
    void Disabled()
    {
        _targetGimmick = null;
    }
}
