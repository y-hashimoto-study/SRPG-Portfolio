using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class BattleManager : MonoBehaviour
{
    public enum GameState
    {
        SelectUnit,
        SelectMove,
        SelectUI,
        ItemTargetSelect,
        AttackTarget,
        Disabled
    }
    public enum RangeType
    {
        Move,
        Attack,
        Item
    }
    public static BattleManager Instance;
    public GameState CurrentGameState {get; private set;} = GameState.SelectUnit;

    private List<PlayerUnit> _allPlayers = new List<PlayerUnit>();
    private List<EnemyUnit> _allEnemies = new List<EnemyUnit>();
    private List<MapCube> _attackTargetRange = new List<MapCube>();
    private List<MapCube> _itemTargetRange = new List<MapCube>();
    public UnitBase SelectedUnit{get; private set;}
    private List<MapCube> _moveMapCubes = new List<MapCube>();
    private Dictionary<Vector2Int,Vector2Int> _moveParentDictionary = new Dictionary<Vector2Int, Vector2Int>();
    private MapCube _oldCube;
    public ItemData CurrentUseItemData;
    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        MapCube.EnterAction += MapCubeEnter;
        MapCube.ExitAction += MapCubeExit;
        MapCube.ClickAction += MapCubeClick;
    }
    private void OnDisable()
    {
        MapCube.EnterAction -= MapCubeEnter;
        MapCube.ExitAction -= MapCubeExit;
        MapCube.ClickAction -= MapCubeClick;
    }
    public void AddAllUnitList(UnitBase unitBase)
    {
        if(unitBase is PlayerUnit player)
        {
            _allPlayers.Add(player);
            return;
        }
        if(unitBase is EnemyUnit enemy)
        {
            _allEnemies.Add(enemy);
            return;
        }
    }
    public void ChangeState(GameState newState)
    {
        CurrentGameState = newState;
    }
    public void MapCubeEnter(MapCube mapCube)
    {
        UIManager.Instance.SetStatusPanel(mapCube.CurrentObject);
        switch (CurrentGameState)
        {
            case GameState.SelectUnit:
            UnitBase unit = mapCube.CurrentUnit;
             if(unit != null && unit.Team == UnitBase.Type.Player && !unit.IsActed)
                {
                    mapCube.CurrentColor = Color.yellow;
                }
            break;
        }
    }
    public void MapCubeExit(MapCube mapCube)
    {
        UIManager.Instance.SetStatusPanel(null);
        mapCube.CurrentColor = mapCube.DefaultColor;
    }
    public void MapCubeClick(MapCube mapCube)
    {
        switch (CurrentGameState)
        {
            case GameState.SelectUnit:
             SelectedUnit = mapCube.CurrentUnit;
             if(SelectedUnit != null && SelectedUnit.Team == UnitBase.Type.Player && !SelectedUnit.IsActed)
                {
                    (_moveMapCubes,_moveParentDictionary)= MapManager.Instance.GetMoveRange(mapCube.Position,SelectedUnit);

                    UIManager.Instance.OpenReturnButton(CancelMove);
                    UIManager.Instance.PushMenu(UIManager.MenuUIStateEnum.MoveSelect);
                    UIManager.Instance.SetRightStatusPanel(SelectedUnit);

                    CameraManager.Instance.TargetCamera(SelectedUnit.transform);

                    CurrentGameState = GameState.SelectMove;
                }
            break;
            
            case GameState.SelectMove:
             if(!_moveMapCubes.Contains(mapCube)) return;
             if(mapCube.CurrentObject != null && mapCube.CurrentObject.GameObject != SelectedUnit.gameObject) return;
                          
             CameraManager.Instance.TargetCamera(SelectedUnit.transform);
             CameraManager.Instance.CanMoveCamera(false);

             CurrentGameState = GameState.Disabled;

             List<Vector2Int> routes = MapManager.Instance.GetRoute(mapCube,_moveParentDictionary);
             StartCoroutine(MapManager.Instance.UnitMoveCoroutine(SelectedUnit,routes,()=>OpenCommandUI(mapCube)));

            break;

            case GameState.AttackTarget:
             CurrentGameState = GameState.Disabled;
                if (_attackTargetRange.Contains(mapCube))
                {
                    mapCube.CurrentObject.Damage(SelectedUnit.Atk,SelectedUnit.IsMagic);
                    MoveFinish();
                }
            break;

            case GameState.ItemTargetSelect:
             if (!_itemTargetRange.Contains(mapCube) || mapCube.CurrentUnit == null)return;
             CurrentGameState = GameState.Disabled;
             (bool canUse,string reason) = CurrentUseItemData.Effect.CanUse(SelectedUnit,mapCube.CurrentUnit);
                if (canUse)
                {
                    UIManager.Instance.OpenConfirmation(() =>
                    {
                        CurrentUseItemData.Effect.UseItem(mapCube.CurrentUnit);
                        MoveFinish();
                    },CurrentUseItemData);
                    UIManager.Instance.PushMenu(UIManager.MenuUIStateEnum.ItemTargetSelect);
                }
                else
                {
                    //メッセージを出す
                    //reasonを表示する
                    //CurrentGameStateを戻す
                }
            break;
        }
    }
    public void OpenCommandUI(MapCube mapCube)
    {
        CameraManager.Instance.CanMoveCamera(true);

        _oldCube = MapManager.Instance.GetMapCube(SelectedUnit.Position);
        _oldCube.CurrentObject = null;
        mapCube.CurrentObject = SelectedUnit;
        SelectedUnit.Position = mapCube.Position;
        SelectedUnit.transform.position = new Vector3(mapCube.transform.position.x, 
        SelectedUnit.transform.position.y, mapCube.transform.position.z);
        MapManager.Instance.SetMapCubesColor(_moveMapCubes,true,null);
        List<MapCube> attackRangeMapCubes = MapManager.Instance.GetAttackRange(mapCube.Position,SelectedUnit.MinAttackRange,SelectedUnit.MaxAttackRange);
        bool isAttack = MapManager.Instance.CanAttackAnyTarget(attackRangeMapCubes,SelectedUnit);
        bool isAction = MapManager.Instance.CanInteractWithGimmick(mapCube.Position);
        UIManager.Instance.OpenCommandPass(isAction,isAttack);
        UIManager.Instance.PushMenu(UIManager.MenuUIStateEnum.Command);

        CurrentGameState = GameState.SelectUI;
    }
    public void MoveFinish()
    {
        SelectedUnit.MoveFinish();
        UnitBase.Type moveFinishTeam = SelectedUnit.Team;
        SelectedUnit = null;
        _oldCube = null;

        UIManager.Instance.ClearMenuStack();
        UIManager.Instance.LockStatusPanel(false);

        MapManager.Instance.SetMapCubesColor(_moveMapCubes,true,null);
        _moveMapCubes.Clear();
        MapManager.Instance.SetMapCubesColor(_attackTargetRange,true,null);
        _attackTargetRange.Clear();
        MapManager.Instance.SetMapCubesColor(_itemTargetRange,true,null);
        _itemTargetRange.Clear();

        bool nextTurn = false;
        switch (moveFinishTeam)
        {
            case UnitBase.Type.Player:
            foreach (UnitBase targetUnit in _allPlayers)
            {
                if(targetUnit.IsActed)continue;
                nextTurn = true;
                break;
            }
            break;
            case UnitBase.Type.Enemy:
            foreach (UnitBase targetUnit in _allEnemies)
            {
                if(targetUnit.IsActed)continue;
                nextTurn = true;
                break;
            }
            break;
        }
        if (nextTurn)
        {
            CurrentGameState = (moveFinishTeam == UnitBase.Type.Player)?GameState.SelectUnit : GameState.Disabled;
        }
        else
        {
            ChangeTurn(moveFinishTeam);
        }
    }
    public void ChangeTurn(UnitBase.Type finishTeam)
    {
        if(finishTeam == UnitBase.Type.Player)
        {
            foreach (UnitBase targetUnit in _allPlayers)
            {
                targetUnit.MoveReset();
            }
            EnemyTurn();
        }
        if(finishTeam == UnitBase.Type.Enemy)
        {
            CurrentGameState = GameState.SelectUnit;
        }
    }
    public void EnemyTurn()
    {
        Debug.Log("EnemyTurn");
        ChangeTurn(UnitBase.Type.Enemy);
    }
    public void ClearCheck()
    {
        if(_allEnemies.Count == 0)
        {
            Debug.Log("クリア");
        }
    }
    public void CancelMove()
    {
        UIManager.Instance.BackMenu();
        CurrentGameState = GameState.SelectUnit;
        MapManager.Instance.SetMapCubesColor(_moveMapCubes,true,null);
        SelectedUnit = null;
    }
    public void DieUnit(Vector2Int diePosition)
    {
        MapCube dieMapCube = MapManager.Instance.GetMapCube(diePosition);
        if(dieMapCube == null) return;
        if(dieMapCube.CurrentUnit is EnemyUnit dieEnemyUnit)
        {
            if(_allEnemies.Contains(dieEnemyUnit))_allEnemies.Remove(dieEnemyUnit);
        }
        dieMapCube.CurrentObject = null;
        ClearCheck();
    }
    public void SetAttackTargetMode()
    {
        UIManager.Instance.CloseCommand();
        UIManager.Instance.OpenReturnButton(()=>MapManager.Instance.SetMapCubesColor(_attackTargetRange,true,null));
        UIManager.Instance.PushMenu(UIManager.MenuUIStateEnum.AttackTargetSelect);

        CurrentGameState = GameState.AttackTarget;
        _attackTargetRange = MapManager.Instance.GetAttackRange(SelectedUnit.Position,SelectedUnit.MinAttackRange,SelectedUnit.MaxAttackRange);
        foreach(MapCube targetCube in _attackTargetRange)
        {
            if(targetCube.CurrentObject != null && targetCube.CurrentObject.GameObject != null)
            {
                if (targetCube.CurrentObject.IsAttackable)
                {
                    targetCube.CurrentColor = Color.blue;
                    targetCube.LockColor = true;
                    continue; 
                }
            }
            targetCube.CurrentColor = Color.red;
            targetCube.LockColor = true;
        }
    }
    public void SetUseItem(ItemBase useItem)
    {
        if(useItem is ItemData item)
        {
            if(item.Effect == null)return;
            CurrentUseItemData = item;
            if (item.TargetSelect)
            {
                _itemTargetRange = MapManager.Instance.GetAttackRange(SelectedUnit.Position,item.MinTargetRange,item.MaxTargetRange);
                foreach (MapCube targetCube in _itemTargetRange)
                {
                    (bool canUse,string reason)= item.Effect.CanUse(SelectedUnit,targetCube.CurrentUnit);
                    targetCube.CurrentColor = (canUse) ? Color.blue : Color.red;
                    targetCube.LockColor = true;
                }
                UIManager.Instance.OpenReturnButton(()=>MapManager.Instance.SetMapCubesColor(_itemTargetRange,true,null));
                UIManager.Instance.PushMenu(UIManager.MenuUIStateEnum.ItemTargetSelect);
                CurrentGameState = GameState.ItemTargetSelect;
            }
            else
            {
                if(item.Effect != null)
                {
                    UIManager.Instance.OpenConfirmation(() =>
                    {
                        CurrentUseItemData.Effect.UseItem(SelectedUnit);
                        SelectedUnit.Inventory.Remove(CurrentUseItemData);
                        UIManager.Instance.CloseConfirmation();
                        CurrentUseItemData = null;
                        MoveFinish();
                    },CurrentUseItemData);
                    UIManager.Instance.PushMenu(UIManager.MenuUIStateEnum.Confirmation);
                }
            }
        }
    }
    public void BackMoveUnit()
    {
        MapManager.Instance.BackMoveUnit(SelectedUnit,_oldCube);
        _oldCube = null;
    }
    public void ClearRangeColors(RangeType range)
    {
        switch (range)
        {
            case RangeType.Move:
             MapManager.Instance.SetMapCubesColor(_moveMapCubes, true, null);
            break;
            case RangeType.Attack:
             MapManager.Instance.SetMapCubesColor(_attackTargetRange, true, null);
            break;
            case RangeType.Item:
             MapManager.Instance.SetMapCubesColor(_itemTargetRange, true, null);
            break;
        }
    }
    public void ReColorTarget(RangeType type)
    {
        switch (type)
        {
            case RangeType.Move:
             MapManager.Instance.SetMapCubesColor(_moveMapCubes, false, Color.blue);
            break;
            case RangeType.Attack:
             MapManager.Instance.SetMapCubesColor(_attackTargetRange, false, Color.blue);
            break;
            case RangeType.Item:
             foreach (MapCube targetCube in _itemTargetRange)
             {
                (bool canUse,string reason)= CurrentUseItemData.Effect.CanUse(SelectedUnit,targetCube.CurrentUnit);
                Color setColor = (canUse) ? Color.blue : Color.red;
                MapManager.Instance.SetMapCubeColor(targetCube,false,setColor);
             }
            break;
        }
    }
    public void Equiped(ItemBase equipedItem)
    {
        if(equipedItem is WeaponData weapon)
        {
            SelectedUnit.Equiped(weapon);
            //元々のものを移すこと
        }
    }
}
