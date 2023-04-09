using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class GridCellObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	GridCell gridCell;

    // Use this for initialization
    void Start()
	{
		int x = (int)transform.position.x;
		int z = (int)transform.position.z;
		gridCell = GridManager.Instance.grid[x, z];
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameController.Instance.pointedGridCell = gridCell;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameController.Instance.beforePointedGridCell = gridCell;
        GameController.Instance.IsChangedPointedGridCell = true;
    }
}

