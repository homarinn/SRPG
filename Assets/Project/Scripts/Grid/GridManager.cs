using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public partial class GridManager : MonoBehaviour, IPointerExitHandler
{
    public static GridManager Instance { get; private set; }

    public AllyUnit ally;
    public EnemyUnit enemy;

    public GameObject allyMoveRangeLayerPrefab;
    public GameObject enemyMoveRangeLayerPrefab;
    public GameObject neutralMoveRangeLayerPrefab;
    public GameObject attackRangeLayerPrefab;

    public GridCell[,] grid;
    public int gridHorizontal = 16;
    public int gridVertical = 16;

    public TerrainType[] terrainTypes;

    public RangeLayer[,] allyMoveRangeLayers;
    public RangeLayer[,] enemyMoveRangeLayers;
    public RangeLayer[,] neutralMoveRangeLayers;
    public RangeLayer[,] attackRangeLayers;
    public RangeLayer[,] allEnemyMoveRangeLayers;
    public RangeLayer[,] allNeutralMoveRangeLayers;

    private readonly Dictionary<Unit, GridCell> unitToGridCellMap = new();

    private Transform cachedTransform;

    public GridCell pointedGridCell;
    public GridCell beforePointedGridCell;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        cachedTransform = transform;

        InitializeGrid();
    }

    private void InitializeGrid()
    {
        InitializesSquares();

        for (int x = 0; x < 16; x++)
        {
            for (int z = 0; z < 16; z++)
            {
                int height = 0;
                if ((x == 7 || x == 8) && (z == 7 || z == 8))
                {
                    height = 1;
                }

                // GridCellの作成
                MakeGridCell(x, z, height);

                // Layerの作成
                MakeLayer(x, z, height, allyMoveRangeLayerPrefab, allyMoveRangeLayers);
                MakeLayer(x, z, height, enemyMoveRangeLayerPrefab, enemyMoveRangeLayers);
                MakeLayer(x, z, height, neutralMoveRangeLayerPrefab, neutralMoveRangeLayers);
                MakeLayer(x, z, height, attackRangeLayerPrefab, attackRangeLayers);
                MakeLayer(x, z, height, enemyMoveRangeLayerPrefab, allEnemyMoveRangeLayers);
                MakeLayer(x, z, height, neutralMoveRangeLayerPrefab, allNeutralMoveRangeLayers);
            }
        }

        SetUnitToCell(ally, 0, 0);
        SetUnitToCell(enemy, 15, 15);
    }

    private void InitializesSquares()
    {
        grid = new GridCell[gridHorizontal, gridVertical];
        allyMoveRangeLayers = new RangeLayer[gridHorizontal, gridVertical];
        enemyMoveRangeLayers = new RangeLayer[gridHorizontal, gridVertical];
        neutralMoveRangeLayers = new RangeLayer[gridHorizontal, gridVertical];
        attackRangeLayers = new RangeLayer[gridHorizontal, gridVertical];
        allEnemyMoveRangeLayers = new RangeLayer[gridHorizontal, gridVertical];
        allNeutralMoveRangeLayers = new RangeLayer[gridHorizontal, gridVertical];
    }

    private void MakeGridCell(int x, int z, int height)
    {
        grid[x, z] = new GridCell(x, z, height, GetTerrainType(x, z));
    }

    private void MakeLayer(int x, int z, int height, GameObject layerPrefab, RangeLayer[,] rangeLayers)
    {
        rangeLayers[x, z] = Instantiate(layerPrefab, new Vector3(x, height + 1.01f, z), Quaternion.Euler(90, 0, 0), cachedTransform).GetComponent<RangeLayer>();
    }

    public void SetUnitToCell(Unit unit, GridCell cell)
    {
        cell.unit = unit;
        unitToGridCellMap[unit] = cell;
    }

    private void SetUnitToCell(Unit unit, int x, int z)
    {
        GridCell cell = grid[x, z];
        cell.unit = unit;
        unitToGridCellMap[unit] = cell;
    }

    private GridCell GetGridCell(int x, int z)
    {
        if (IsInBounds(x, z))
        {
            return grid[x, z];
        }

        return null;
    }

    public HashSet<GridCell> GetMoveableCells(Unit unit)
    {
        GridCell startCell = GetUnitGridCell(unit);

        startCell.ResetCost();

        HashSet<GridCell> moveableCells = new ();
        Queue<GridCell> toVisit = new ();

        toVisit.Enqueue(startCell);
        moveableCells.Add(startCell);

        while (toVisit.Count > 0)
        {
            GridCell current = toVisit.Dequeue();
            foreach (GridCell neighborCell in GetNeighborCells(current))
            {
                if (moveableCells.Contains(neighborCell) ||
                    !CanUnitMoveToCell(unit, current, neighborCell))
                {
                    continue;
                }

                int moveCost = unit.MoveCost(neighborCell.terrainType);
                if (current.gCost + moveCost <= unit.moveRange)
                {
                    neighborCell.gCost = current.gCost + moveCost;
                    moveableCells.Add(neighborCell);
                    toVisit.Enqueue(neighborCell);
                }
            }
        }

        return moveableCells;
    }

    public HashSet<GridCell> GetAttackableCells(Unit unit)
    {
        HashSet<GridCell> moveableCells = unit.GetMoveableCells();

        if (moveableCells.Count == 0)
        {
            moveableCells.Add(GetUnitGridCell(unit));
        }

        HashSet<GridCell> attackableCells = new ();

        int minAttackRange = unit.GetMinAttackRange();
        int maxAttackRange = unit.GetMaxAttackRange();

        foreach (GridCell moveableCell in moveableCells)
        {
            int moveableX = moveableCell.x;
            int moveableZ = moveableCell.z;

            for (int range = minAttackRange; range <= maxAttackRange; range++)
            {
                for (int x = moveableX - range; x <= moveableX + range; x++)
                {
                    for (int z = moveableZ - range; z <= moveableZ + range; z++)
                    {
                        if (Mathf.Abs(x - moveableX) + Mathf.Abs(z - moveableZ) >= minAttackRange &&
                            Mathf.Abs(x - moveableX) + Mathf.Abs(z - moveableZ) <= maxAttackRange)
                        {
                            GridCell attackCell = GetGridCell(x, z);

                            if (attackCell != null && !attackableCells.Contains(attackCell))
                            {
                                attackableCells.Add(attackCell);
                            }
                        }
                    }
                }
            }
        }

        return attackableCells;
    }

    // 再起的に実行される
    private void AddAttackableCellsOutsideMoveableRecursively(GridCell originCell, GridCell centerCell, Unit unit, ref HashSet<GridCell> notSearchCells, ref HashSet<GridCell> attackableCells)
    {
        foreach (GridCell neighborCell in GetNeighborCells(centerCell))
        {
            if (!notSearchCells.Contains(neighborCell))
            {
                notSearchCells.Add(neighborCell);

                int distance = GetDistance(originCell, neighborCell);
                if (unit.GetMaxAttackRange() >= distance)
                {
                    if (unit.GetMinAttackRange() <= distance && !IsOccupiedByAlly(unit, neighborCell))
                    {
                        attackableCells.Add(neighborCell);
                    }

                    if (unit.GetMaxAttackRange() > distance)
                    {
                        // 攻撃範囲が2以上の時に対応できるよう再帰的に実行
                        AddAttackableCellsOutsideMoveableRecursively(originCell, neighborCell, unit, ref notSearchCells, ref attackableCells);
                    }
                }
            }
        }
    }

    private HashSet<GridCell> GetNeighborCells(GridCell cell)
    {
        HashSet<GridCell> neighborCells = new ();

        AddNeighborCell(cell.x + 1, cell.z, ref neighborCells);
        AddNeighborCell(cell.x, cell.z + 1, ref neighborCells);
        AddNeighborCell(cell.x - 1, cell.z, ref neighborCells);
        AddNeighborCell(cell.x, cell.z - 1, ref neighborCells);

        return neighborCells;
    }

    // GetNeighborCellsと密結合、コードを短くするため用意
    private void AddNeighborCell(int x, int z, ref HashSet<GridCell> neighborCells)
    {
        if (IsInBounds(x, z))
        {
            neighborCells.Add(grid[x, z]);
        }
    }

    public int GetDistance(GridCell cellA, GridCell cellB)
    {
        return Mathf.Abs(cellA.x - cellB.x) + Mathf.Abs(cellA.z - cellB.z);
    }

    private TerrainType GetTerrainType(int x, int z)
    {
        // TODO: 座標によって返すものを修正
        return terrainTypes[0];
    }

    private RangeLayer GetMoveRangeLayerForUnit(Unit unit, GridCell moveableCell)
    {
        if (unit is AllyUnit)
        {
            return allyMoveRangeLayers[moveableCell.x, moveableCell.z];
        }
        else if (unit is EnemyUnit)
        {
            return enemyMoveRangeLayers[moveableCell.x, moveableCell.z];
        }
        else if (unit is NeutralUnit)
        {
            return neutralMoveRangeLayers[moveableCell.x, moveableCell.z];
        }

        return null;
    }

    private RangeLayer GetAttackRangeLayerForUnit(Unit unit, GridCell attackableCell)
    {
        return attackRangeLayers[attackableCell.x, attackableCell.z];
    }

    public void ShowMoveAndAttackRange(Unit unit)
    {
        HashSet<GridCell> moveableCells = unit.GetMoveableCells();
        HashSet<GridCell> attackableCells = unit.GetAttackableCells();

        // 移動範囲と攻撃範囲が重なるセルを除外する
        attackableCells.ExceptWith(moveableCells);

        foreach (GridCell moveableCell in moveableCells)
        {
            GetMoveRangeLayerForUnit(unit, moveableCell).ShowLayer();
        }

        foreach (GridCell attackableCell in attackableCells)
        {
            GetAttackRangeLayerForUnit(unit, attackableCell).ShowLayer();
        }
    }

    public void HideMoveAndAttackRange(Unit unit)
    {
        HashSet<GridCell> moveableCells = unit.GetMoveableCells();
        HashSet<GridCell> attackableCells = unit.GetAttackableCells();

        // 移動範囲と攻撃範囲が重なるセルを除外する
        attackableCells.ExceptWith(moveableCells);

        foreach (GridCell moveableCell in moveableCells)
        {
            GetMoveRangeLayerForUnit(unit, moveableCell).HideLayer();
        }

        foreach (GridCell attackableCell in attackableCells)
        {
            GetAttackRangeLayerForUnit(unit, attackableCell).HideLayer();
        }
    }

    public bool IsOccupiedByEnemy(Unit unit, GridCell cell)
    {
        return cell.unit != null && unit.IsEnemy(cell.unit);
    }

    public bool IsOccupiedByAlly(Unit unit, GridCell cell)
    {
        return cell.unit != null && unit.IsAlly(cell.unit);
    }

    public bool IsOccupiedBySelf(Unit unit, GridCell cell)
    {
        return cell.unit != null && unit == cell.unit;
    }

    public bool IsInBounds(int x, int z)
    {
        return x >= 0 && x < gridHorizontal && z >= 0 && z < gridVertical;
    }

    private bool CanUnitMoveToCell(Unit unit, GridCell fromCell, GridCell toCell)
    {
        return toCell.IsMoveable &&
               Mathf.Abs(toCell.height - fromCell.height) <= unit.jumpPower &&
               !IsOccupiedByEnemy(unit, toCell);
    }

    public bool IsInCells(GridCell cell, HashSet<GridCell> cells)
    {
        return cells.Contains(cell);
    }

    public GridCell GetUnitGridCell(Unit unit)
    {
        return unitToGridCellMap[unit];
    }

    public HashSet<GridCell> GetUnitNeighborCells(Unit unit)
    {
        return GetNeighborCells(GetUnitGridCell(unit));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameController.Instance.pointedGridCell = null;
    }
}
