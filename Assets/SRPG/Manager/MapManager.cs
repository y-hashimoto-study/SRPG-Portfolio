using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    public enum GameState
    {
        SelectUnit,
        SelectMove,
        SelectUI,
        ItemTargetSelect,
        AttackTarget,
        Disabled
    }
    public GameState CurrentGameState {get; private set;} = GameState.SelectUnit;
    [SerializeField] private GameObject _mapCubeParent;
    private Dictionary<Vector2Int, MapCube> _mapCubeDictionary = new Dictionary<Vector2Int, MapCube>();
    public UnitBase SelectedUnit{get; private set;}
    private List<MapCube> _moveMapCubes = new List<MapCube>();
    private Dictionary<Vector2Int,Vector2Int> _moveParentDictionary = new Dictionary<Vector2Int, Vector2Int>();
    private MapCube _oldCube;
    private List<PlayerUnit> _allPlayers = new List<PlayerUnit>();
    private List<EnemyUnit> _allEnemies = new List<EnemyUnit>();
    private List<MapCube> _attackRange = new List<MapCube>();
    [SerializeField] private float _unitMoveSpeed = 30f;
    public ItemData CurrentUseItemData;
    private const int GridSpace = 10;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitializeMap();
    }
    private void InitializeMap()
    {
        if(_mapCubeParent == null) return;
        MapCube[] mapCubes =  _mapCubeParent.GetComponentsInChildren<MapCube>();

        foreach (MapCube cube in mapCubes)
        {
            Vector2Int cubePosition = new Vector2Int(
                Mathf.RoundToInt(cube.transform.position.x),
                Mathf.RoundToInt(cube.transform.position.z)
                );
            cube.Position = cubePosition;
            if (!_mapCubeDictionary.ContainsKey(cubePosition))
            {
                _mapCubeDictionary.Add(cubePosition, cube);
            }
        }     
        Debug.Log($"{_mapCubeDictionary.Count}個のマスを認識しました！");
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
    public void MapCubeEnter(MapCube mapCube)
    {
        UIManager.Instance.SetStatusPanel(mapCube.CurrentObject);
        switch (CurrentGameState)
        {
            case GameState.SelectUnit:
             if(mapCube.CurrentUnit is PlayerUnit player && !player.IsActed)
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
             if(mapCube.CurrentUnit is PlayerUnit player && !player.IsActed)
                {
                    CurrentGameState = GameState.SelectMove;
                    SelectedUnit = player;
                    SearchMoveRange(mapCube.Position,SelectedUnit.Mov);
                    UIManager.Instance.OpenReturnButton(ReturnMove);
                    UIManager.Instance.PushMenu(UIManager.MenuUIStateEnum.MoveSelect);
                    CameraManager.Instance.TargetCamera(player.transform);

                    UIManager.Instance.SetRightStatusPanel(player);
                }
            break;

            case GameState.SelectMove:
             if(!_moveMapCubes.Contains(mapCube)) return;
             if(mapCube.CurrentObject != null && mapCube.CurrentObject.GameObject != SelectedUnit.gameObject) return;
             CurrentGameState = GameState.Disabled;
             CameraManager.Instance.TargetCamera(SelectedUnit.transform);
             CameraManager.Instance.CanMoveCamera(false);
             List<Vector2Int> routes = GetRoute(mapCube);
             StartCoroutine(UnitMoveCoroutine(SelectedUnit,routes,()=>CommandUI(mapCube)));
            break;

            case GameState.AttackTarget:
             CurrentGameState = GameState.Disabled;
                if (_attackRange.Contains(mapCube))
                {
                    mapCube.CurrentObject.Damage(SelectedUnit.Atk,SelectedUnit.IsMagic);
                    MoveFinish();
                }
            break;

            case GameState.ItemTargetSelect:
             if (!_attackRange.Contains(mapCube) || mapCube.CurrentUnit == null)return;
             CurrentGameState = GameState.Disabled;
             (bool canUse,string reason) = CurrentUseItemData.Effect.CanUse(SelectedUnit,mapCube.CurrentUnit);
                if (canUse)
                {
                    UIManager.Instance.OpenConfirmation(() =>
                    {
                        CurrentUseItemData.Effect.UseItem(mapCube.CurrentUnit);
                        MoveFinish();
                    });
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
    public void ChangeState(GameState newState)
    {
        CurrentGameState = newState;
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
    public MapCube GetMapCube(Vector2Int position)
    {
        if (_mapCubeDictionary.ContainsKey(position))
        {
            return _mapCubeDictionary[position];
        }
        return null;
    }
    public List<MapCube> GetNeighborsCubes(Vector2Int startPosition)
    {
        List<MapCube> neighborsCubes = new List<MapCube>();
        Vector2Int[] neighborsVector2Int =
        {
            new Vector2Int(0,GridSpace),new Vector2Int(0,-GridSpace),new Vector2Int(GridSpace,0),new Vector2Int(-GridSpace,0)
        };
        foreach(Vector2Int neighbor in neighborsVector2Int)
        {
            Vector2Int checkPosition = startPosition + neighbor;
            if(_mapCubeDictionary.ContainsKey(checkPosition))
            {
                neighborsCubes.Add(_mapCubeDictionary[checkPosition]);
            }
        } 
        return neighborsCubes;
    }
    public void SearchMoveRange(Vector2Int startPos, int move)
    {
        foreach(MapCube oldCube in _moveMapCubes)
        {
            oldCube.CurrentColor = oldCube.DefaultColor;
        }
        _moveMapCubes.Clear();
        _moveParentDictionary.Clear();

        Queue<Vector2Int> waitQueue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> finishDictionary = new Dictionary<Vector2Int, int>();
        waitQueue.Enqueue(startPos);
        MapCube startCube = GetMapCube(startPos);
        finishDictionary.Add(startPos,0);

        while (waitQueue.Count > 0)
        {
            Vector2Int currentVector2Int = waitQueue.Dequeue();
            int currentCost = finishDictionary[currentVector2Int];
            foreach (MapCube nextCube in GetNeighborsCubes(currentVector2Int))
            {
                if (nextCube.CurrentObject != null && SelectedUnit != null && nextCube.CurrentObject.Team != SelectedUnit.Team) continue;
                int nextCost = currentCost + nextCube.MapCost;
                if(nextCost > move) continue; 
                if (finishDictionary.ContainsKey(nextCube.Position))
                {
                    if(nextCost >= finishDictionary[nextCube.Position]) continue;
                    finishDictionary[nextCube.Position] = nextCost;
                    waitQueue.Enqueue(nextCube.Position);
                    _moveParentDictionary[nextCube.Position] = currentVector2Int;
                    continue;
                }
                finishDictionary.Add(nextCube.Position,nextCost);
                waitQueue.Enqueue(nextCube.Position);
                _moveMapCubes.Add(nextCube);
                _moveParentDictionary.Add(nextCube.Position,currentVector2Int);
            }
        }
        _moveMapCubes.RemoveAll(cube => cube.CurrentObject != null);
        _moveMapCubes.Add(startCube);
        foreach(MapCube moveCube in _moveMapCubes)
        {
            moveCube.CurrentColor = Color.blue;
            moveCube.LockColor = true;
        }
    }
    public void ReturnMove()
    {
        UIManager.Instance.BackMenu();
        CurrentGameState = GameState.SelectUnit;
        ColorMoveRange(true);
        SelectedUnit = null;
    }
    public List<MapCube> AttackRange(Vector2Int startPosition,int minRange,int maxRange)
    {
        List<MapCube> attackMapCubes = new List<MapCube>();
        Vector2Int targetPosition = new Vector2Int();
        for(int i = -maxRange; i <= maxRange; i++)
        {
            for(int j = -maxRange; j <= maxRange; j++)
            {
                int distance = Mathf.Abs(i) + Mathf.Abs(j);
                if(distance >= minRange && distance <= maxRange)
                {
                    targetPosition.x = startPosition.x + i * GridSpace;
                    targetPosition.y = startPosition.y + j * GridSpace;
                    MapCube targetCube = GetMapCube(targetPosition);
                    if(targetCube != null)attackMapCubes.Add(targetCube);
                }
            }
        }
        _attackRange = attackMapCubes;
        return attackMapCubes;
    }
    public bool CanAttack(List<MapCube> rangeCubes)
    {
        foreach(MapCube cube in rangeCubes)
        {
            if(cube.CurrentObject == null || cube.CurrentObject.GameObject == null) continue;
            if(cube.CurrentObject.IsAttackable)
            {
                if(SelectedUnit.Team == cube.CurrentObject.Team)continue;
                return true;
            }
        }
        return false;
    }
    public bool CanAction(Vector2Int startPosition)
    {
        List<MapCube> targetCubes= GetNeighborsCubes(startPosition);
        //startCube. MapCubeに何か落ちているかを持たせる?
        foreach (MapCube targetCube in targetCubes)
        {
            if(targetCube.CurrentObject == null || targetCube.CurrentObject.GameObject == null) continue;
            if(targetCube.CurrentGimmick != null && targetCube.CurrentGimmick.IsActionable) return true;
        }
        return false;
    }
    public void SetAttackMode()
    {
        UIManager.Instance.CloseCommand();
        CurrentGameState = GameState.AttackTarget;
        UIManager.Instance.OpenReturnButton(ClearColorAttackMapCube);
        UIManager.Instance.PushMenu(UIManager.MenuUIStateEnum.AttackTargetSelect);
        List<MapCube> attackCubes = AttackRange(SelectedUnit.Position,SelectedUnit.MinAttackRange,SelectedUnit.MaxAttackRange);
        foreach(MapCube targetCube in attackCubes)
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
    public void MoveFinish()
    {
        SelectedUnit.MoveFinish();
        UnitBase.Type moveFinishTeam = SelectedUnit.Team;
        SelectedUnit = null;
        _oldCube = null;
        UIManager.Instance.ClearMenuStack();
        UIManager.Instance.LockStatusPanel(false);
        
        if(_moveMapCubes.Count != 0)
        {
            ColorMoveRange(true);
            _moveMapCubes.Clear();
        }
        if(_attackRange.Count != 0)
        {
            ClearColorAttackMapCube();
            _attackRange.Clear();
        }
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
    public void ClearColorAttackMapCube()
    {
        if(_attackRange.Count == 0) return;
        foreach(MapCube targetCube in _attackRange)
        {
            targetCube.LockColor = false;
            targetCube.CurrentColor = targetCube.DefaultColor;
        }
    }
    public void DieUnit(Vector2Int diePosition)
    {
        MapCube dieCube = GetMapCube(diePosition);
        if(dieCube == null) return;
        if(dieCube.CurrentUnit is EnemyUnit dieEnemyUnit)
        {
            if(_allEnemies.Contains(dieEnemyUnit))_allEnemies.Remove(dieEnemyUnit);
        }
        dieCube.CurrentObject = null;
        ClearCheck();
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
    public void BackUnit()
    {
        if(SelectedUnit == null || _oldCube == null)return;
        MapCube targetCube = GetMapCube(SelectedUnit.Position);
        if(targetCube != null) targetCube.CurrentObject = null;
        SelectedUnit.Position = _oldCube.Position;
        SelectedUnit.transform.position = new Vector3(_oldCube.Position.x,_oldCube.transform.position.y,_oldCube.Position.y);
        _oldCube.CurrentObject = SelectedUnit;
        _oldCube = null;
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
    public void ColorMoveRange(bool Defalt)
    {
        if(_moveMapCubes == null) return;
        foreach (MapCube targetCube in _moveMapCubes)
        {
            targetCube.LockColor = false;
            Color paintColor = (Defalt) ? targetCube.DefaultColor : Color.blue;
            targetCube.CurrentColor = paintColor;
            targetCube.LockColor = !Defalt;
        }
    }
    public List<Vector2Int> GetRoute(MapCube targetCube)
    {
        List<Vector2Int> routes = new List<Vector2Int>();
        Vector2Int currentPosition = targetCube.Position;
        while (_moveParentDictionary.ContainsKey(currentPosition))
        {
            MapCube routeCube = GetMapCube(currentPosition);
            if(routeCube != null)
            {
                routes.Add(routeCube.Position);
            }
            currentPosition = _moveParentDictionary[routeCube.Position];
        } 
        routes.Reverse();
        return routes;
    }
    private IEnumerator UnitMoveCoroutine(UnitBase moveUnit,List<Vector2Int>routes,System.Action completeAction)
    {
        MapCube beforCube = GetMapCube(SelectedUnit.Position);
        foreach (Vector2Int route in routes)
        {
            MapCube targetCube = GetMapCube(route);
            Vector3 targetPosition = targetCube.transform.position;
            float yPosition = targetPosition.y - beforCube.transform.position.y;
            targetPosition.y = moveUnit.transform.position.y + yPosition;
            while (Vector3.Distance(moveUnit.transform.position,targetPosition) > 0.01f)
            {
                moveUnit.transform.position = Vector3.MoveTowards(moveUnit.transform.position,targetPosition,_unitMoveSpeed * Time.deltaTime);
                yield return null;
            }
            moveUnit.transform.position = targetPosition;
            beforCube = targetCube;
        }
        if(completeAction != null)
        {
            completeAction.Invoke();
        }
    }
    private void CommandUI(MapCube mapCube)
    {
        CameraManager.Instance.CanMoveCamera(true);
        CurrentGameState = GameState.SelectUI;
        _oldCube = GetMapCube(SelectedUnit.Position);
        _oldCube.CurrentObject = null;
        mapCube.CurrentObject = SelectedUnit;
        SelectedUnit.Position = mapCube.Position;
        //SelectedUnit.transform.position = new Vector3(mapCube.Position.x,mapCube.transform.position.y,mapCube.Position.y);
        ColorMoveRange(true);
        List<MapCube> attackRangeMapCubes = AttackRange(mapCube.Position,SelectedUnit.MinAttackRange,SelectedUnit.MaxAttackRange);
        bool isAttack = CanAttack(attackRangeMapCubes);
        bool isAction = CanAction(mapCube.Position);
        UIManager.Instance.OpenCommandPass(isAction,isAttack);
        UIManager.Instance.PushMenu(UIManager.MenuUIStateEnum.Command);
    }
    public void UseItem(ItemBase useItem)
    {
        if(useItem is ItemData item)
        {
            if(item.Effect == null)return;
            CurrentUseItemData = item;
            if (item.TargetSelect)
            {
                List<MapCube> targetCubes = AttackRange(SelectedUnit.Position,item.MinTargetRange,item.MaxTargetRange);
                foreach (MapCube targetCube in targetCubes)
                {
                    (bool canUse,string reason)= item.Effect.CanUse(SelectedUnit,targetCube.CurrentUnit);
                    targetCube.CurrentColor = (canUse) ? Color.blue : Color.red;
                    targetCube.LockColor = true;
                }
                UIManager.Instance.OpenReturnButton(ClearColorAttackMapCube);
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
                    });
                    UIManager.Instance.PushMenu(UIManager.MenuUIStateEnum.Confirmation);
                }
            }
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
