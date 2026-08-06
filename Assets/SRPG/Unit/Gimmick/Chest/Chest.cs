using UnityEngine;
using System.Threading.Tasks;
public class Chest : GimmickBase,IActionable
{
    public string ActionName => "あける";
    [SerializeField] public string _completeText => $"{_getItem.name}を手に入れました";
    [SerializeField] private ItemBase _getItem;
    private bool _isOpened = false;
    public Task<bool> ActionGimmick(PlayerUnit player)
    {
        if(_isOpened) return Task.FromResult(false);
        _isOpened = true;
        return BattleManager.Instance.StartItemGet(player,_getItem,_completeText,CompleteAction);
    }
    public void CompleteAction()
    {
        BattleManager.Instance.DestoryMapObject(Position);
        Destroy(gameObject);
    }
    public void RestAction()
    {
        _isOpened = false;
    }
}
