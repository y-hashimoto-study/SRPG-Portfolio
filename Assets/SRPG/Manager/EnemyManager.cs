using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    private int turnInt = 0;
    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void StartEnemyTurn()
    {
        turnInt = 0;
        BattleManager.Instance.EnemyTurn(turnInt);
    }
    public void UpdateEnemyTurn()
    {
        turnInt++;
        BattleManager.Instance.EnemyTurn(turnInt);
    }
    public List<MapCube> GetSerchAttackTarget(EnemyUnit enemy)
    {
        Vector2Int startPosition = enemy.Position;
        MapCube startCube = MapManager.Instance.GetMapCube(startPosition);

        List<MapCube> TargetUnitMapCube = MapManager.Instance.GetSerchTargetUnit(startPosition,enemy);
        if(TargetUnitMapCube.Count == 0) return TargetUnitMapCube;

        List<(MapCube cube, int cost)> checkCubes = new List<(MapCube, int)>();
        foreach (MapCube mapCube in TargetUnitMapCube)
        {
            if (mapCube.CurrentObject != null && mapCube.CurrentObject.IsAttackable && mapCube.CurrentObject.Team != enemy.Team)
            {
                int currentCost = Mathf.Abs(mapCube.Position.x - startPosition.x) + Mathf.Abs(mapCube.Position.y - startPosition.y);
                checkCubes.Add((mapCube, currentCost));
            }
        }
        checkCubes.Sort((a, b) => a.cost.CompareTo(b.cost));
        List<MapCube> targetCubes = new List<MapCube>();
        foreach ((MapCube,int) cubeCost in checkCubes)
        {   
            targetCubes.Add(cubeCost.Item1);
        }
        return targetCubes;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetCubes"></param>
    /// <param name="enemy"></param>
    /// <returns>(移動先のMapCube,moveParentDictionary,攻撃対象)</returns>
    public (MapCube,Dictionary<Vector2Int,Vector2Int>,MapCube) GetMoveCube(List<MapCube> targetCubes,EnemyUnit enemy)
    {
        if(targetCubes == null) return (null,new Dictionary<Vector2Int, Vector2Int>(),null);
        Vector2Int startPosition = enemy.Position;
        MapCube startCube = MapManager.Instance.GetMapCube(startPosition);
        
        (List<MapCube>moveRanges,Dictionary<Vector2Int,Vector2Int>moveParentDictionary,Dictionary<Vector2Int,int>costDictionary) 
        = MapManager.Instance.GetMoveRange(startPosition,enemy);
  
        foreach(MapCube targetCube in targetCubes)
        {
            Vector2Int targetPosition = targetCube.Position;
            List<MapCube> attakTargetRanges = MapManager.Instance.GetAttackRange(targetPosition,enemy.MinAttackRange,enemy.MaxAttackRange);
            if(attakTargetRanges.Count == 0) continue;
            foreach (MapCube moveCube in attakTargetRanges)
            {
                if(moveRanges.Contains(moveCube) && (moveCube.CurrentObject == null || moveCube.Position == startPosition))
                {
                    return (moveCube,moveParentDictionary,targetCube);
                }
            }
        }
        
        MapCube nearTargetMapCUbe = targetCubes[0];
        MapCube bestTargetCube = startCube;
        int befordistance = int.MaxValue;

        foreach (MapCube moveRangeCube in moveRanges)
        {
            if(moveRangeCube.CurrentObject != null && moveRangeCube.Position != startPosition) continue;
            int distance = Mathf.Abs(moveRangeCube.Position.x - nearTargetMapCUbe.Position.x) +
            Mathf.Abs(moveRangeCube.Position.y - nearTargetMapCUbe.Position.y);
            if(distance < befordistance)
            {
                bestTargetCube = moveRangeCube;
                befordistance = distance;
            }
            else if(distance == befordistance)
            {
                if(UnityEngine.Random.Range(0,2) == 0) bestTargetCube = moveRangeCube;
            }
        }
        return (bestTargetCube,moveParentDictionary,null);
    }
    public void AttackEnemy(EnemyUnit enemy,IMapObject target)
    {
        target.Damage(enemy.Atk,enemy.IsMagic);
        BattleManager.Instance.MoveFinish();
    }
    public void MoveEnemy(EnemyUnit enemy,MapCube mapCube,System.Action completeAction)
    {
        MapCube oldCube = MapManager.Instance.GetMapCube(enemy.Position);
        oldCube.CurrentObject = null;

        mapCube.CurrentObject = enemy;
        enemy.Position = mapCube.Position;
        enemy.transform.position = new Vector3(mapCube.transform.position.x, 
        enemy.transform.position.y, mapCube.transform.position.z);
        completeAction.Invoke();
    }
}
