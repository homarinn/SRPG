using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class GridManager
{
    public HashSet<GridCell> FindPath(Unit unit, GridCell targetCell)
    {
        GridCell startCell = unitToGridCellMap[unit];

        HashSet<GridCell> openSet = new();
        HashSet<GridCell> closedSet = new();

        startCell.gCost = 0;
        startCell.hCost = GetDistance(startCell, targetCell);
        startCell.fCost = startCell.gCost + startCell.hCost;

        openSet.Add(startCell);

        while (openSet.Count > 0)
        {
            GridCell currentCell = openSet.OrderBy(cell => cell.fCost).ThenBy(cell => cell.hCost).First();

            if (currentCell == targetCell)
            {
                return RetracePath(startCell, targetCell);
            }

            openSet.Remove(currentCell);
            closedSet.Add(currentCell);

            foreach (GridCell neighborCell in GetNeighborCells(currentCell))
            {
                if (closedSet.Contains(neighborCell) ||
                    !CanUnitMoveToCell(unit, currentCell, neighborCell)
                    )
                {
                    continue;
                }

                int moveCost = currentCell.gCost + unit.MoveCost(neighborCell.terrainType);
                if (!openSet.Contains(neighborCell) || moveCost < neighborCell.gCost)
                {
                    neighborCell.gCost = moveCost;
                    neighborCell.hCost = GetDistance(neighborCell, targetCell);
                    neighborCell.fCost = neighborCell.gCost + neighborCell.hCost;
                    neighborCell.parent = currentCell;

                    if (!openSet.Contains(neighborCell))
                    {
                        openSet.Add(neighborCell);
                    }
                }
            }
        }

        return null;
    }



    private HashSet<GridCell> RetracePath(GridCell startCell, GridCell endCell)
    {
        List<GridCell> path = new();
        GridCell currentCell = endCell;

        while (currentCell != startCell)
        {
            path.Add(currentCell);
            currentCell = currentCell.parent;
        }

        path.Add(startCell); 
        path.Reverse(); // Reverse the list to get the correct order from start to end

        return new HashSet<GridCell>(path);
    }
}

