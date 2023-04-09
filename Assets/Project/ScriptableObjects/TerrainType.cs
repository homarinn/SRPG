using UnityEngine;

[CreateAssetMenu(fileName = "NewTerrainType", menuName = "ScriptableObject/TerrainType")]
public class TerrainType : ScriptableObject
{
    public int defaultMoveCost = 1;
    public bool isMoveable = true;
}
