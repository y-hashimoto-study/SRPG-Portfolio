using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public enum MenuUIStateEnum
    {
        MoveSelect,
        Command,
        Inventory,
        AttackTargetSelect,
        ItemTargetSelect,
        Confirmation,
        Message
    }
    public static UIManager Instance;
    [SerializeField] private Command _command;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private Confirmation _confirmation;
    [SerializeField] private ReturnButton _return;
    [SerializeField] private LeftStatusPanel _leftStatusPanel;
    [SerializeField] private RightStatusPanel _rightStatusPanel;
    [SerializeField] private Message _Message;
    [SerializeField] private PopUpMessage _popUpMessage;
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
        _command.gameObject.SetActive(false);
        _inventory.gameObject.SetActive(false);
        _confirmation.gameObject.SetActive(false);
        _return.gameObject.SetActive(false);
        _Message.gameObject.SetActive(false);
        _popUpMessage.gameObject.SetActive(false);

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
                case MenuUIStateEnum.Message:
                 CloseMessage();
                break;
                case MenuUIStateEnum.Confirmation:
                 CloseConfirmation();
                break;
            }
        }
        MenuStack.Push(newState);
    }
    public void ClearMenuStack()
    {
        MenuStack.Clear();
        _command.gameObject.SetActive(false);
        _inventory.gameObject.SetActive(false);
        _confirmation.gameObject.SetActive(false);
        _return.gameObject.SetActive(false);
        _Message.gameObject.SetActive(false);
    }
    /// <summary>
    /// Pushを外からすること
    /// </summary>
    /// <param name="returnMethod"></param>
    public void OpenReturnButton(Action returnMethod)
    {
        _return.gameObject.SetActive(true);
        _return.ReturnAction = () =>
        {
            if(BattleManager.Instance.CurrentGameState == BattleManager.GameState.Disabled) return;
             if(returnMethod != null)returnMethod();
             BackMenu();
        };
    }
    public void CloseReturnButton()
    {
        _return.gameObject.SetActive(false);
    }
    public void OpenCommandPass(IActionable actionable,bool isAttack)
    {
        _command.gameObject.SetActive(true);
        _command.OpenCommand(actionable,isAttack);
    }
    public void CloseCommand()
    {
        _command.gameObject.SetActive(false);
    }
    public void OpenInventory(UnitBase unit,Action<ItemBase> customClickAction = null)
    {
        _inventory.gameObject.SetActive(true);
        _inventory.OpenInventory(unit,customClickAction);
    }
    public void CloseInventory()
    {
        _inventory.gameObject.SetActive(false);
    }
    public void SetConfirmation(Action yesAction, string confirmationText,Action noAction = null)
    {
        _confirmation.gameObject.SetActive(true);
        _confirmation.SetUp(confirmationText);
        _confirmation.YesAction = yesAction;
        if(noAction == null)
        {
            _confirmation.NoAction = () => BackMenu();
        }
        else
        {
            _confirmation.NoAction = noAction;
        }
    }
    public void CloseConfirmation()
    {
        _confirmation.gameObject.SetActive(false);
    }
    public void SetMessage(string message ,Sprite itemSprite,Action completeAction)
    {
        _Message.gameObject.SetActive(true);
        _Message.SetUp(message,itemSprite,completeAction);
    }
    public void CloseMessage()
    {
        _Message.gameObject.SetActive(false);
    }
    public void SetStatusPanel(IMapObject imapObject)
    {
        if(imapObject == null || imapObject.GameObject == null)
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
    public void RefreshInventory(UnitBase unit)
    {
        if(MenuStack.Peek() != MenuUIStateEnum.Inventory) return;
        _inventory.gameObject.SetActive(false);
        OpenInventory(unit);
    }
    public void PopUpMessage(string message)
    {
        _popUpMessage.gameObject.SetActive(true);
        _popUpMessage.PopUp(message);
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
             _command.gameObject.SetActive(false);
            break;

            case MenuUIStateEnum.AttackTargetSelect:
             CloseReturnButton();
            break;

            case MenuUIStateEnum.Inventory:
             CloseInventory();
            break;
            
            case MenuUIStateEnum.ItemTargetSelect:
             CloseReturnButton();
            break;

            case MenuUIStateEnum.Message:
             CloseMessage();
            break;

            case MenuUIStateEnum.Confirmation:
             CloseConfirmation();
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
                 BattleManager.Instance.ReColorTarget(BattleManager.RangeType.Move);
                break;

                case MenuUIStateEnum.Command:
                 BattleManager.Instance.ChangeState(BattleManager.GameState.SelectUI);
                 _command.gameObject.SetActive(true);
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
                case MenuUIStateEnum.Message:
                 //なり得ないように設計すること
                 Debug.Log($"エラーこの状態にはならないはず{beforeStack}");;
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
