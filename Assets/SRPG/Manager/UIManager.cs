using UnityEngine;
using System;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public enum MenuUIStateEnum
    {
        MoveSelect,
        Command,
        Inventory,
        AttackTargetSelect,
        ItemTargetSelect,
        Confirmation
    }
    public static UIManager Instance;
    [SerializeField] private Command _commandPanel;
    [SerializeField] private Inventory _inventoryPanel;
    [SerializeField] private Confirmation _confirmationPanel;
    [SerializeField] private ReturnButton _returnPanel;
    [SerializeField] private LeftStatusPanel _leftStatusPanel;
    [SerializeField] private RightStatusPanel _rightStatusPanel;
    private Stack<MenuUIStateEnum> MenuStack = new Stack<MenuUIStateEnum>();
    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void Start()
    {
        MenuStack.Clear();
        _commandPanel.gameObject.SetActive(false);
        _inventoryPanel.gameObject.SetActive(false);
        _confirmationPanel.gameObject.SetActive(false);
        _returnPanel.gameObject.SetActive(false);
        _leftStatusPanel.SetStatusActive(false);
        _rightStatusPanel.SetStatusActive(false);
    }
    public void PushMenu(MenuUIStateEnum newState)
    {
        if(MenuStack.Count > 0)
        {
            MenuUIStateEnum currentState = MenuStack.Peek();
            switch (currentState)
            {
                case MenuUIStateEnum.MoveSelect:
                 CloseReturnButton();
                break;
                case MenuUIStateEnum.Command:
                 CloseCommand();
                break;
                case MenuUIStateEnum.AttackTargetSelect:
                 CloseReturnButton();
                break;
                case MenuUIStateEnum.Inventory:
                 CloseInventory();
                break;
                case MenuUIStateEnum.ItemTargetSelect:
                 BattleManager.Instance.ClearRangeColors(BattleManager.RangeType.Item);
                 CloseReturnButton();
                break;
                case MenuUIStateEnum.Confirmation:
                 CloseConfirmation();//なり得ないよう設計すること
                 Debug.Log("エラーこの状態にはならないはず Confiremation");
                break;
            }
        }
        MenuStack.Push(newState);
    }
    public void ClearMenuStack()
    {
        MenuStack.Clear();
        _commandPanel.gameObject.SetActive(false);
        _inventoryPanel.gameObject.SetActive(false);
        _confirmationPanel.gameObject.SetActive(false);
        _returnPanel.gameObject.SetActive(false);
    }
    /// <summary>
    /// Pushを外からすること
    /// </summary>
    /// <param name="returnMethod"></param>
    public void OpenReturnButton(Action returnMethod)
    {
        _returnPanel.gameObject.SetActive(true);
        _returnPanel.ReturnAction = () =>
        {
            if(BattleManager.Instance.CurrentGameState == BattleManager.GameState.Disabled) return;
             if(returnMethod != null)returnMethod();
             BackMenu();
        };
    }
    public void CloseReturnButton()
    {
        _returnPanel.gameObject.SetActive(false);
    }
    public void OpenCommandPass(bool isAction,bool isAttack)
    {
        _commandPanel.gameObject.SetActive(true);
        _commandPanel.OpenCommand(isAction,isAttack);
    }
    public void CloseCommand()
    {
        _commandPanel.gameObject.SetActive(false);
    }
    public void OpenInventory(UnitBase unit)
    {
        _inventoryPanel.gameObject.SetActive(true);
        _inventoryPanel.OpenInventory(unit);
    }
    public void CloseInventory()
    {
        _inventoryPanel.gameObject.SetActive(false);
    }
    public void OpenConfirmation(Action action,ItemBase item)
    {
        _confirmationPanel.gameObject.SetActive(true);
        _confirmationPanel.SetUp(item.name + "を使いますか?");
        _confirmationPanel.YesAction += action;
        _confirmationPanel.NoAction += BackMenu;

    }
    public void CloseConfirmation()
    {
        _confirmationPanel.gameObject.SetActive(false);
    }
    public void SetStatusPanel(IMapObject imapObject)
    {
        if(imapObject == null)
        {
            _rightStatusPanel.SetStatusActive(false);
            _leftStatusPanel.SetStatusActive(false);
            return;
        }
        if(imapObject is PlayerUnit)
        {
            _rightStatusPanel.SetStatusActive(true);
            _leftStatusPanel.SetStatusActive(false);
            _rightStatusPanel.SetUp(imapObject);
        }
        else
        {
            _leftStatusPanel.SetStatusActive(true);
            _rightStatusPanel.SetStatusActive(false);
            _leftStatusPanel.SetUp(imapObject);
        }
    }
    public void SetLeftStatusPanel(IMapObject imapObject)
    {
        _leftStatusPanel.SetStatusActive(true);
        _leftStatusPanel.SetUp(imapObject);
        _leftStatusPanel.LockChenge = true;
    }
    public void SetRightStatusPanel(IMapObject imapObject)
    {
        _rightStatusPanel.SetStatusActive(true);
        _rightStatusPanel.SetUp(imapObject);
        _rightStatusPanel.LockChenge = true;
    }
    public void LockStatusPanel(bool lockChenge)
    {
        _rightStatusPanel.LockChenge = lockChenge;
        _leftStatusPanel.LockChenge = lockChenge;
    }
    public void BackMenu()
    {
        if(MenuStack.Count == 0) return;
        MenuUIStateEnum currentStack = MenuStack.Pop();
        switch (currentStack)
        {
            case MenuUIStateEnum.MoveSelect:
             CloseReturnButton();
            break;
            
            case MenuUIStateEnum.Command:
             _commandPanel.gameObject.SetActive(false);
            break;

            case MenuUIStateEnum.AttackTargetSelect:
             CloseReturnButton();
            break;

            case MenuUIStateEnum.Inventory:
             _inventoryPanel.gameObject.SetActive(false);
            break;
            
            case MenuUIStateEnum.ItemTargetSelect:
             CloseReturnButton();
            break;
            case MenuUIStateEnum.Confirmation:
             _confirmationPanel.gameObject.SetActive(false);
            break;
        }
        if(MenuStack.Count > 0)
        {
            MenuUIStateEnum beforeStack = MenuStack.Peek();
            switch (beforeStack)
            {
                case MenuUIStateEnum.MoveSelect:
                 BattleManager.Instance.ChangeState(BattleManager.GameState.SelectMove);
                 OpenReturnButton(BattleManager.Instance.CancelMove);
                 BattleManager.Instance.BackMoveUnit();
                 BattleManager.Instance.ClearRangeColors(BattleManager.RangeType.Move);
                break;

                case MenuUIStateEnum.Command:
                 BattleManager.Instance.ChangeState(BattleManager.GameState.SelectUI);
                 _commandPanel.gameObject.SetActive(true);
                break;

                case MenuUIStateEnum.AttackTargetSelect:
                 BattleManager.Instance.ChangeState(BattleManager.GameState.AttackTarget);
                 OpenReturnButton(()=>BattleManager.Instance.ClearRangeColors(BattleManager.RangeType.Attack));
                break;

                case MenuUIStateEnum.Inventory:
                 OpenInventory(BattleManager.Instance.SelectedUnit);
                break;

                case MenuUIStateEnum.ItemTargetSelect:
                 CloseConfirmation();
                 BattleManager.Instance.ReColorTarget(BattleManager.RangeType.Item);
                 OpenReturnButton(()=>BattleManager.Instance.ClearRangeColors(BattleManager.RangeType.Item));
                 BattleManager.Instance.ChangeState(BattleManager.GameState.ItemTargetSelect);
                break;

                case MenuUIStateEnum.Confirmation:
                 //なり得ないように設計すること
                 Debug.Log($"エラーこの状態にはならないはず{beforeStack}");
                break;
                 
            }
        }
        else
        {
            LockStatusPanel(false);
            BattleManager.Instance.ChangeState(BattleManager.GameState.SelectUnit);
        }
    }

}
