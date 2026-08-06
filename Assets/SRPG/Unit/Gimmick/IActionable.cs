using UnityEngine;
using System.Threading.Tasks;
public interface IActionable
{
    string ActionName{get;}
    Task<bool> ActionGimmick(PlayerUnit player);
    void CompleteAction();
    void RestAction();
}
