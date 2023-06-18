using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }
    private List<Unit> unitList;
    private List<Unit> friendlyUnitList;
    private List<Unit> enemyUnitList;

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError("multiple instances of UnitManager");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        unitList = new List<Unit>();
        friendlyUnitList = new List<Unit>();
        enemyUnitList = new List<Unit>();
    }

    private void Start()
    {
        Unit.OnAnyUnitSpawned += Unit_OnAnyUnitSpawned;
        Unit.OnAnyUnitDied += Unit_OnAnyUnitDied;
    }

    private void Unit_OnAnyUnitSpawned(object sender, EventArgs e)
    {
        Unit unit = sender as Unit;
        if (unit.IsEnemy())
        {
            enemyUnitList.Add(unit);
        }
        else
        {
            friendlyUnitList.Add(unit);
        }
        unitList.Add(unit);
    }

    private void Unit_OnAnyUnitDied(object sender, EventArgs e)
    {
        Unit unit = sender as Unit;
        if (unit.IsEnemy())
        {
            enemyUnitList.Remove(unit);
        }
        else
        {
            friendlyUnitList.Remove(unit);
        }
        unitList.Remove(unit);
    }

    public List<Unit> GetUnitList()
    {
        return unitList;
    }

    public List<Unit> GetEnemyUnitList()
    {
        return enemyUnitList;
    }

    public List<Unit> GetFriendlyUnitList()
    {
        return friendlyUnitList;
    }

    public int GetFriendlyUnitsWithActionPointsCount()
    {
        return GetCountOfUnitsWithActionPointsRemaining(friendlyUnitList);
    }

    private int GetCountOfUnitsWithActionPointsRemaining(List<Unit> units)
    {
        List<Unit> unitsWithActionPoints = units.FindAll(unit => unit.GetActionPoints() > 0);
        return unitsWithActionPoints.Count;
    }
}
