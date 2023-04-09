public class GridCell
{
    public int x;
    public int z;
    public int height;
    public GridCell parent;
    public TerrainType terrainType;
    public Unit unit;

    public int gCost = 0;
    public int hCost = 0;
    public int fCost = 0;

    public GridCell(int x, int z, int height, TerrainType terrainType)
    {
        this.x = x;
        this.z = z;
        this.height = height;
        this.terrainType = terrainType;
    }

    public bool IsMoveable { get { return terrainType.isMoveable; } }

    public void ResetCost()
    {
        gCost = 0;
        hCost = 0;
        fCost = 0;
    }
}

