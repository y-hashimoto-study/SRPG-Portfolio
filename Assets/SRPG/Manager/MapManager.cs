using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    [SerializeField] private GameObject _mapCubeParent;
    private Dictionary<Vector2Int, MapCube> _mapCubeDictionary = new Dictionary<Vector2Int, MapCube>();
    [SerializeField] private float _unitMoveSpeed = 30f;
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
    public (List<MapCube>,Dictionary<Vector2Int,Vector2Int>,Dictionary<Vector2Int,int>)GetMoveRange(Vector2Int startPos, UnitBase selectedUnit)
    {
        List<MapCube> moveMapCubes = new List<MapCube>();
        Dictionary<Vector2Int, Vector2Int> moveParentDictionary = new Dictionary<Vector2Int, Vector2Int>();

        int move = selectedUnit.Mov;
        
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
                if (nextCube.CurrentObject != null && selectedUnit != null && nextCube.CurrentObject.Team != selectedUnit.Team) continue;
                int nextCost = currentCost + nextCube.MapCost;
                if(nextCost > move) continue; 
                if (finishDictionary.ContainsKey(nextCube.Position))
                {
                    if(nextCost >= finishDictionary[nextCube.Position]) continue;
                    finishDictionary[nextCube.Position] = nextCost;
                    waitQueue.Enqueue(nextCube.Position);
                    moveParentDictionary[nextCube.Position] = currentVector2Int;
                    continue;
                }
                finishDictionary.Add(nextCube.Position,nextCost);
                waitQueue.Enqueue(nextCube.Position);
                moveMapCubes.Add(nextCube);
                moveParentDictionary.Add(nextCube.Position,currentVector2Int);
            }
        }
        moveMapCubes.RemoveAll(cube => cube.CurrentObject != null);
        moveMapCubes.Add(startCube);
        
        return (moveMapCubes,moveParentDictionary,finishDictionary);
    }
    public List<MapCube> GetAttackRange(Vector2Int startPosition,int minRange,int maxRange)
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
        return attackMapCubes;
    }
    public bool CanAttackAnyTarget(List<MapCube> rangeCubes,UnitBase unit)
    {
        foreach(MapCube cube in rangeCubes)
        {
            if(cube.CurrentObject == null || cube.CurrentObject.GameObject == null) continue;
            if(cube.CurrentObject.IsAttackable)
            {
                if(unit.Team == cube.CurrentObject.Team)continue;
                return true;
            }
        }
        return false;
    }
    public bool CanInteractWithGimmick(Vector2Int startPosition)
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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetCube">移動の目標地点</param>
    /// <param name="moveParentDictionary">開始地点に向けて各マスの一つ前のマス</param>
    /// <returns></returns>
    public List<Vector2Int> GetRoute(MapCube targetCube,Dictionary<Vector2Int,Vector2Int> moveParentDictionary)
    {
        List<Vector2Int> routes = new List<Vector2Int>();
        Vector2Int currentPosition = targetCube.Position;
        while (moveParentDictionary.ContainsKey(currentPosition))
        {
            MapCube routeCube = GetMapCube(currentPosition);
            if(routeCube != null)
            {
                routes.Add(routeCube.Position);
            }
            currentPosition = moveParentDictionary[routeCube.Position];
        } 
        routes.Reverse();
        return routes;
    }
    public IEnumerator UnitMoveCoroutine(UnitBase moveUnit,List<Vector2Int>routes,System.Action completeAction)
    {
        MapCube beforCube = GetMapCube(moveUnit.Position);
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
     public void SetMapCubeColor(MapCube mapCube,bool Defalt,Color? color)
    {
        if(mapCube == null) return;
            mapCube.LockColor = false;
            Color paintColor = (Defalt) ? mapCube.DefaultColor :(color == null) ? Color.blue : color.Value;
            mapCube.CurrentColor = paintColor;
            mapCube.LockColor = !Defalt;
    }
     public void SetMapCubesColor(List<MapCube> mapCubes,bool Defalt,Color? color)
    {
        if(mapCubes.Count == 0) return;
        foreach (MapCube mapCube in mapCubes)
        {
            mapCube.LockColor = false;
            Color paintColor = (Defalt) ? mapCube.DefaultColor :(color == null) ? Color.blue : color.Value;
            mapCube.CurrentColor = paintColor;
            mapCube.LockColor = !Defalt;
        }
    }
    public void BackMoveUnit(UnitBase unit,MapCube oldMapCube)
    {
        if(unit == null || oldMapCube == null)return;
        MapCube currentMapCube = GetMapCube(unit.Position);
        if(currentMapCube != null) currentMapCube.CurrentObject = null;
        unit.Position = oldMapCube.Position;
        float yPosition = oldMapCube.transform.position.y - currentMapCube.transform.position.y;
        unit.transform.position = new Vector3(oldMapCube.Position.x,unit.transform.position.y-yPosition,oldMapCube.Position.y);
        oldMapCube.CurrentObject = unit;
    }

    /// <summary>
    /// Unitがサーチ範囲にあるかを探す
    /// 敵側の索敵
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="enemy"></param>
    /// <returns></returns>
    public List<MapCube> GetSerchTargetUnit(Vector2Int startPos, EnemyUnit enemy)
    {
        List<MapCube> TargetUnitMapCubes = new List<MapCube>();
        int serch = enemy.SerchRange;
        
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
                int nextCost = currentCost + nextCube.MapCost;
                if(nextCost > serch) continue; 

                if (nextCube.CurrentObject != null && enemy != null && nextCube.CurrentObject.Team != enemy.Team)
                {
                    TargetUnitMapCubes.Add(nextCube);
                    continue;
                }
                if (finishDictionary.ContainsKey(nextCube.Position))
                {
                    if(nextCost >= finishDictionary[nextCube.Position]) continue;
                    finishDictionary[nextCube.Position] = nextCost;
                    waitQueue.Enqueue(nextCube.Position);
                    continue;
                }
                finishDictionary.Add(nextCube.Position,nextCost);
                waitQueue.Enqueue(nextCube.Position);
            }
        }
        return TargetUnitMapCubes;
    }
}
