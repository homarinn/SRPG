using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public abstract class Unit : MonoBehaviour
{
    public int maxHealth = 1;
    public int health = 1;
    public int strength = 0;
    public int magic = 0;
    public int defense = 0;
	public int dexterity = 0;
    public int agility = 0;
	public int resist = 0;
    public int luck = 0;
	public int moveRange = 1;
    public int jumpPower = 1;
    public int level = 1;
    public int currentExperience = 0; // 現在の経験値
    public int experience = 0; // 倒した時に獲得できる経験値
    public int defaultCanActionCount = 1;

    public bool IsSelected { get { return this == UnitController.Instance.selectedUnit; } }
    public bool IsMoved { get; set; } = false;
    public bool IsActioned { get; set; } = false;
    public bool IsTurnEnded { get { return canActionCount == 0;  } }

    private int canActionCount;

    public Dictionary<TerrainType, int> moveCostEachTerrainType = new();

    public HashSet<GridCell> cachedMoveableCells;
    public HashSet<GridCell> cachedAttackableCells;

    public Transform cachedTransform;

    public List<Unit> canTalkUnits = new ();

    private void Awake()
	{
        InitializeParameters();
    }

    private void Start()
    {
        cachedTransform = transform;
        canActionCount = defaultCanActionCount;
    }

    protected abstract void InitializeParameters();

    public bool IsEnemy(Unit other)
    {
        return !IsAlly(other);
    }

    public bool IsAlly(Unit other)
    {
        return (this is AllyUnit && other is AllyUnit) || (this is EnemyUnit && other is EnemyUnit) || (this is NeutralUnit && other is NeutralUnit);
    }

    public int MoveCost(TerrainType terrainType)
    {
        if (moveCostEachTerrainType.ContainsKey(terrainType))
        {
            return moveCostEachTerrainType[terrainType];
        }

        return terrainType.defaultMoveCost;
    }

    // MinとMaxを定義
    public abstract int[] GetAttackRange();

    public int GetMinAttackRange()
    {
        return GetAttackRange()[0];
    }

    public int GetMaxAttackRange()
    {
        return GetAttackRange()[1];
    }

    public void MoveStart()
    {

    }

    public void MoveEnd()
    {
        IsMoved = true;
    }

    public void MoveBack()
    {
        IsMoved = false;
    }

    public HashSet<GridCell> GetMoveableCells()
    {
        if (IsMoved || IsTurnEnded)
        {
            return new HashSet<GridCell>();
        }

        if (cachedMoveableCells == null)
        {
            cachedMoveableCells = GridManager.Instance.GetMoveableCells(this);
        }

        return cachedMoveableCells;
    }

    public HashSet<GridCell> GetAttackableCells()
    {
        if (IsActioned || IsTurnEnded)
        {
            return new HashSet<GridCell>();
        } else if (IsMoved)
        {
            return GridManager.Instance.GetAttackableCells(this);
        }

        if (cachedAttackableCells == null)
        {
            cachedAttackableCells = GridManager.Instance.GetAttackableCells(this);
        }

        return cachedAttackableCells;
    }

    public void ResetCellsCache()
    {
        cachedMoveableCells = null;
        cachedAttackableCells = null;
    }

    public void TurnStart()
    {
        canActionCount = defaultCanActionCount;
        gameObject.layer = LayerMask.NameToLayer("Unit");
        IsMoved = false;
        IsActioned = false;
    }

    public void TurnEnd()
    {
        ResetCellsCache();
        canActionCount -= 1;

        if (IsTurnEnded)
        {
            gameObject.layer = LayerMask.NameToLayer("TurnEndedUnit");
        } else
        {
            IsMoved = false;
            IsActioned = false;
        }
    }
}
